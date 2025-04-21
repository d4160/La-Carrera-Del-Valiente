using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlatformerDynamicController2D : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Movement")]
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float deceleration = 15f;
    [SerializeField] private float maxHorizontalSpeed = 8f;
    private float currentHorizontalInput = 0f;
    private float currentHorizontalSpeed = 0f;
    private bool facingRight = true;

    [Header("Jumping")]
    [SerializeField] private float jumpForce = 15f;
    [SerializeField] private int maxJumps = 2; // Max saltos (1 = normal, 2 = doble salto, etc.)
    [SerializeField] private float fallMultiplier = 2.5f; // Gravedad aumentada al caer
    [SerializeField] private float lowJumpMultiplier = 2f; // Multiplicador si se suelta el botón de salto pronto
    [SerializeField] private float jumpCutOffVelocity = 0.5f; // Mínima velocidad Y para aplicar lowJumpMultiplier
    private int jumpsRemaining;
    private bool jumpRequested = false;
    private bool jumpCancelled = false;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckRadius = 0.2f;
    [SerializeField] private LayerMask groundLayer;
    private bool isGrounded = false;
    private bool wasGroundedLastFrame = false;

    [Header("Coyote Time")]
    [SerializeField] private float coyoteTimeDuration = 0.1f;
    private float coyoteTimeCounter = 0f;

    [Header("Jump Buffer")]
    [SerializeField] private float jumpBufferDuration = 0.1f;
    private float jumpBufferCounter = 0f;

    [Header("Dashing")]
    [SerializeField] private float dashSpeed = 20f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 0.5f;
    [SerializeField] private int maxAirDashes = 1;
    [SerializeField] private Color dashColor = Color.cyan;
    [SerializeField] private float dashCameraShakeIntensity = 5f;
    [SerializeField] private float dashCameraShakeDuration = 0.1f;
    private bool isDashing = false;
    private bool canDash = true;
    private int dashesRemaining;
    private float timeSinceLastDash = Mathf.Infinity;
    private Vector2 dashDirection;
    private float originalGravityScale;
    private Color originalSpriteColor;
    private Coroutine dashCoroutine;

    // Public properties for external scripts (AI, Input)
    public bool IsGrounded => isGrounded;
    public bool IsFacingRight => facingRight;
    public Vector2 Velocity => rb.linearVelocity;

    void Awake()
    {
        // Obtener componentes si no están asignados
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
        if (groundCheckPoint == null)
        {
            // Intentar encontrarlo por nombre, o crear uno si no existe
            Transform found = transform.Find("GroundCheck");
            if (found)
            {
                groundCheckPoint = found;
            }
            else
            {
                GameObject gc = new GameObject("GroundCheck");
                gc.transform.SetParent(transform);
                gc.transform.localPosition = new Vector3(0, -GetComponent<Collider2D>().bounds.extents.y, 0);
                groundCheckPoint = gc.transform;
                Debug.LogWarning("GroundCheckPoint not assigned. Created one automatically at the bottom of the collider.", this);
            }
        }

        originalGravityScale = rb.gravityScale;
        originalSpriteColor = spriteRenderer.color;
        facingRight = transform.localScale.x > 0; // Asumir inicio mirando a la derecha
    }

    void Start()
    {
        // Resetear estados iniciales
        ResetJumps();
        ResetDashes();
    }

    void Update()
    {
        HandleTimers();
        HandleJumpInputLogic(); // Maneja buffer y request
        HandleDashCooldown();

        // No mover ni saltar si está en dash
        if (isDashing) return;

        // Procesar Jump si se solicitó y es posible
        TryExecuteJump();

    }

    void FixedUpdate()
    {
        wasGroundedLastFrame = isGrounded;
        CheckGrounded();

        // Resetear saltos y dashes al tocar el suelo
        if (isGrounded && !wasGroundedLastFrame)
        {
            ResetJumps();
            ResetDashes();
            coyoteTimeCounter = coyoteTimeDuration; // Asegura que se pueda saltar inmediatamente al aterrizar
        }


        // No aplicar físicas normales durante el dash
        if (isDashing) return;

        HandleHorizontalMovement();
        ApplyJumpPhysicsModifiers(); // Control de altura variable y caída rápida
    }

    // --- Public Methods for Controllers (Input/AI) ---

    /// <summary>
    /// Establece la intención de movimiento horizontal.
    /// </summary>
    /// <param name="horizontalInput">Valor entre -1 (izquierda) y 1 (derecha).</param>
    public void SetHorizontalInput(float horizontalInput)
    {
        if (isDashing) return; // No permitir cambiar input durante dash
        currentHorizontalInput = horizontalInput;
    }

    /// <summary>
    /// Señala la intención de saltar. Se procesará en Update/FixedUpdate.
    /// </summary>
    public void RequestJump()
    {
        if (isDashing) return;
        jumpRequested = true;
        jumpBufferCounter = jumpBufferDuration; // Activar buffer
    }

    /// <summary>
    /// Señala que se ha dejado de presionar el botón de salto.
    /// </summary>
    public void RequestJumpCancel()
    {
        if (isDashing) return;
        jumpCancelled = true;
        // La lógica de cortar el salto está en ApplyJumpPhysicsModifiers
    }

    /// <summary>
    /// Intenta iniciar un Dash en la dirección dada.
    /// </summary>
    /// <param name="direction">La dirección del dash (debe ser normalizada si es necesario).</param>
    public void RequestDash(Vector2 direction)
    {
        if (canDash && !isDashing && direction != Vector2.zero)
        {
            // Comprobar si quedan dashes (infinitos en suelo, limitados en aire)
            if (isGrounded || dashesRemaining > 0)
            {
                if (!isGrounded)
                {
                    dashesRemaining--;
                }
                StartDash(direction.normalized);
            }
        }
    }

    // --- Internal Logic ---

    private void HandleHorizontalMovement()
    {
        // Calcular velocidad objetivo basada en el input
        float targetSpeed = currentHorizontalInput * maxHorizontalSpeed;

        // Determinar aceleración o deceleración
        float accel = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;

        // Si hay input y el personaje se mueve en dirección opuesta, usar aceleración (para cambio rápido de dirección)
        if (Mathf.Abs(currentHorizontalInput) > 0.01f && Mathf.Sign(targetSpeed) != Mathf.Sign(currentHorizontalSpeed) && currentHorizontalSpeed != 0)
        {
            accel = acceleration; // O un valor mayor si se quiere un cambio de dirección más brusco
        }
        // Si no hay input y el personaje se está moviendo, usar deceleración
        else if (Mathf.Abs(currentHorizontalInput) < 0.01f && Mathf.Abs(currentHorizontalSpeed) > 0.01f)
        {
            accel = deceleration;
        }


        // Mover suavemente la velocidad actual hacia la velocidad objetivo
        currentHorizontalSpeed = Mathf.MoveTowards(currentHorizontalSpeed, targetSpeed, accel * Time.fixedDeltaTime);

        // Aplicar la velocidad horizontal al Rigidbody
        rb.linearVelocity = new Vector2(currentHorizontalSpeed, rb.linearVelocity.y);

        // Voltear el sprite según la dirección del input (si hay input significativo)
        if (Mathf.Abs(currentHorizontalInput) > 0.1f)
        {
            Flip(currentHorizontalInput > 0);
        }
        // O voltear según la velocidad si no hay input (para deslizarse)
        else if (Mathf.Abs(currentHorizontalInput) < 0.1f && Mathf.Abs(rb.linearVelocity.x) > 0.1f)
        {
            // No voltear si se está deteniendo, mantener la última dirección
            // Flip(rb.velocity.x > 0); // Descomentar si se quiere voltear al deslizar
        }
    }

    private void Flip(bool faceRight)
    {
        if (facingRight != faceRight)
        {
            facingRight = faceRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    private void CheckGrounded()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);

        // Manejar contador de Coyote Time
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTimeDuration;
        }
        else
        {
            coyoteTimeCounter -= Time.fixedDeltaTime; // Usar fixedDeltaTime aquí porque la comprobación es en FixedUpdate
        }
    }

    private void HandleTimers()
    {
        // Contadores que dependen de Time.deltaTime (Update)
        jumpBufferCounter -= Time.deltaTime;
    }

    private void HandleDashCooldown()
    {
        if (!canDash)
        {
            timeSinceLastDash += Time.deltaTime;
            if (timeSinceLastDash >= dashCooldown)
            {
                canDash = true;
            }
        }
    }


    private void HandleJumpInputLogic()
    {
        // No hacer nada si jumpRequested no está activo
        if (!jumpRequested) return;

        // Si el buffer está activo, mantenemos la solicitud
        if (jumpBufferCounter > 0f)
        {
            // La ejecución se intenta en TryExecuteJump
        }
        else
        {
            // Si el buffer expiró, cancelamos la solicitud
            jumpRequested = false;
        }

        // Importante: jumpCancelled se resetea aquí después de ser potencialmente usado en ApplyJumpPhysicsModifiers
        jumpCancelled = false; // Resetear estado de cancelación para el próximo frame
    }

    private void TryExecuteJump()
    {
        // Intentar saltar si se solicitó (jumpRequested) y está en buffer (jumpBufferCounter > 0)
        // O si aún está en Coyote Time
        if (jumpRequested && (jumpBufferCounter > 0f))
        {
            // Condiciones para poder saltar: (Estar en suelo O en Coyote Time) Y tener saltos restantes
            if ((isGrounded || coyoteTimeCounter > 0f) && jumpsRemaining > 0)
            {
                PerformJump();
                jumpRequested = false; // Consumir la solicitud
                jumpBufferCounter = 0f; // Consumir el buffer
                coyoteTimeCounter = 0f; // Consumir coyote time si se usó
            }
            // Saltar en el aire (si quedan multisaltos)
            else if (!isGrounded && jumpsRemaining > 0 && maxJumps > 1 && wasGroundedLastFrame == false) // Añadido chequeo de !wasGroundedLastFrame para evitar doble salto inmediato al dejar plataforma
            {
                PerformJump();
                jumpRequested = false; // Consumir la solicitud
                jumpBufferCounter = 0f; // Consumir el buffer
            }
        }

        // Si no se pudo saltar pero se solicitó, la solicitud permanece activa mientras dure el buffer.
        // Si el buffer expira (manejado en HandleTimers y HandleJumpInputLogic), jumpRequested se volverá false.
    }


    private void PerformJump()
    {
        // Aplicar fuerza de salto inmediata modificando la velocidad Y
        // Asegura que la velocidad X no se vea afectada
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        jumpsRemaining--;
        jumpCancelled = false; // Asegurar que no se cancele inmediatamente
    }

    private void ApplyJumpPhysicsModifiers()
    {
        // Gravedad aumentada al caer para un salto más rápido y pesado
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.fixedDeltaTime;
        }
        // Reducir la altura del salto si se suelta el botón pronto (y todavía está subiendo)
        else if (rb.linearVelocity.y > jumpCutOffVelocity && jumpCancelled) // Usar jumpCancelled aquí
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * (1f - lowJumpMultiplier * Time.fixedDeltaTime)); // Forma más suave de reducirlo
            // Alternativa más brusca: rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
        }

        // Resetear jumpCancelled después de aplicarlo una vez (o al tocar suelo/empezar a caer)
        if (rb.linearVelocity.y <= 0 || isGrounded)
        {
            jumpCancelled = false;
        }
    }

    private void StartDash(Vector2 direction)
    {
        if (dashCoroutine != null) StopCoroutine(dashCoroutine); // Detener dash anterior si existe
        dashCoroutine = StartCoroutine(DashCoroutine(direction));
        timeSinceLastDash = 0f; // Reiniciar contador para cooldown
        canDash = false; // Deshabilitar dash hasta que termine cooldown
    }

    private IEnumerator DashCoroutine(Vector2 direction)
    {
        isDashing = true;
        dashDirection = direction; // Guardar dirección por si es necesaria

        // Guardar estado y aplicar efectos de dash
        originalGravityScale = rb.gravityScale;
        rb.gravityScale = 0f;
        rb.linearVelocity = direction * dashSpeed; // Aplicar velocidad de dash
        spriteRenderer.color = dashColor;

        // Camera Shake (si existe CameraManager)
        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.ShakeCamera(dashCameraShakeDuration, dashCameraShakeIntensity);
        }
        else
        {
            Debug.LogWarning("CameraManager.Instance not found. Cannot shake camera.");
        }

        yield return new WaitForSeconds(dashDuration);

        // Terminar dash y restaurar estado
        rb.gravityScale = originalGravityScale;
        isDashing = false;
        spriteRenderer.color = originalSpriteColor;
        // Opcional: Frenar después del dash o dejar que la física normal tome control
        // rb.velocity = Vector2.zero; // Frenado brusco
        rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.5f, rb.linearVelocity.y * 0.2f); // Frenado suave

        dashCoroutine = null;
    }


    private void ResetJumps()
    {
        jumpsRemaining = maxJumps;
    }

    private void ResetDashes()
    {
        // No resetear 'canDash' aquí, eso lo maneja el cooldown
        dashesRemaining = maxAirDashes; // Resetea solo los dashes aéreos
    }

    // Gizmos para visualizar el Ground Check en el editor
    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
        }
    }
}