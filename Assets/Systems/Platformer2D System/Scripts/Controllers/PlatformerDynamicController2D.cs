using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlatformerDynamicController2D : MonoBehaviour
{
    [Header("Rigidbody Settings")]
    [SerializeField] private float _gravityStrength = 20f; // Magnitud de la gravedad personalizada
    [SerializeField] private Vector2 _gravityDirection = Vector2.down; // Direcci�n de la gravedad
    [SerializeField] private float _linearDrag = 4f; // Fricci�n cuando est� en el suelo y no se mueve
    [SerializeField] private float _airLinearDrag = 1f; // Resistencia al aire
    [SerializeField] private float _maxFallSpeed = 30f; // Velocidad m�xima de ca�da

    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 8f; // Aceleraci�n o fuerza de movimiento
    [SerializeField] private float _maxMoveSpeed = 15f; // Velocidad horizontal m�xima
    [SerializeField] private float _dashSpeed = 25f;
    [SerializeField] private float _dashDuration = 0.15f;

    [Header("Ground Check")]
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _groundCheckDistance = 0.1f;
    [SerializeField] private Vector2 _groundCheckOffset = Vector2.zero; // Offset relativo al centro del collider
    [SerializeField] private Vector2 _groundCheckBoxSizeMultiplier = new Vector2(0.95f, 0.1f); // Multiplicador del tama�o para el boxcast

    private Rigidbody2D _rb;
    private Collider2D _collider;

    private Vector2 _inputDirection;
    private bool _isGrounded;
    private bool _isDashing;
    private float _dashTimer;
    private Vector2 _dashDirection;

    // Propiedades p�blicas
    public Vector2 InputDirection
    {
        get => _inputDirection;
        // Normalizar aqu� si es necesario, aunque el input system suele darlo normalizado
        set => _inputDirection = value;
    }

    public bool IsGrounded => _isGrounded;
    public bool IsDashing => _isDashing;
    public Vector2 Velocity => _rb.linearVelocity; // Obtener la velocidad real del Rigidbody
    public Vector2 GravityDirection => _gravityDirection.normalized;

    // Debug
    private RaycastHit2D _debugGroundHit;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _collider = GetComponent<Collider2D>();

        // Configuraci�n inicial del Rigidbody Din�mico
        _rb.bodyType = RigidbodyType2D.Dynamic;
        _rb.gravityScale = 0; // Usaremos nuestra propia gravedad manual
        _rb.constraints = RigidbodyConstraints2D.FreezeRotation; // Evitar que rote
    }

    private void Update()
    {
        // Actualizar estado del Dash
        if (_isDashing)
        {
            _dashTimer -= Time.deltaTime;
            if (_dashTimer <= 0f)
            {
                _isDashing = false;
                // Opcional: aplicar una velocidad residual o volver al control normal
                // _rb.velocity = _dashDirection * _moveSpeed; // Ejemplo
            }
        }
    }

    private void FixedUpdate()
    {
        // 1. Chequear si est� en el suelo
        CheckGrounded();

        // 2. Manejar el Dash (tiene prioridad)
        if (_isDashing)
        {
            // Durante el dash, la velocidad es fija y no se aplica gravedad ni input normal
            _rb.linearVelocity = _dashDirection * _dashSpeed;
            _rb.linearDamping = 0; // Sin fricci�n durante el dash
            return; // Salir temprano si est� en dash
        }

        // 3. Calcular direcci�n perpendicular a la gravedad (para movimiento horizontal relativo)
        Vector2 gravityNormal = _gravityDirection.normalized;
        Vector2 perpendicularDirection = GetPerpendicularDirection(gravityNormal);

        // 4. Aplicar Input de Movimiento (como una fuerza o cambio de velocidad)
        // Proyectar el input sobre la direcci�n perpendicular a la gravedad
        float horizontalInput = Vector2.Dot(_inputDirection, perpendicularDirection);
        Vector2 targetHorizontalVelocity = perpendicularDirection * horizontalInput * _maxMoveSpeed;

        // Usar la velocidad actual para el componente paralelo a la gravedad
        Vector2 currentParallelVelocity = Vector2.Dot(_rb.linearVelocity, gravityNormal) * gravityNormal;
        Vector2 currentPerpendicularVelocity = _rb.linearVelocity - currentParallelVelocity;

        // Interpolar o acelerar hacia la velocidad objetivo perpendicular
        // Esto da una sensaci�n m�s suave que establecer la velocidad directamente
        float accelerationFactor = _isGrounded ? _moveSpeed : _moveSpeed * 0.8f; // Menor control en el aire (opcional)
        Vector2 force = (targetHorizontalVelocity - currentPerpendicularVelocity) * accelerationFactor;
        _rb.AddForce(force); // Usar AddForce para un movimiento m�s basado en f�sica

        // Alternativa: Establecer velocidad directamente (m�s "snappy")
        // _rb.velocity = targetHorizontalVelocity + currentParallelVelocity;


        // 5. Aplicar Gravedad Manual si no est� en el suelo
        if (!_isGrounded)
        {
            _rb.AddForce(gravityNormal * _gravityStrength);

            // Limitar velocidad de ca�da
            float verticalSpeed = Vector2.Dot(_rb.linearVelocity, gravityNormal);
            if (verticalSpeed > _maxFallSpeed)
            {
                _rb.linearVelocity -= gravityNormal * (verticalSpeed - _maxFallSpeed);
            }

            _rb.linearDamping = _airLinearDrag; // Aplicar resistencia del aire
        }
        else
        {
            // Aplicar fricci�n si est� en el suelo y no hay input horizontal significativo
            if (Mathf.Abs(horizontalInput) < 0.1f)
            {
                _rb.linearDamping = _linearDrag; // Alta fricci�n para detenerse
            }
            else
            {
                _rb.linearDamping = 0f; // Sin fricci�n base si se est� moviendo activamente
            }

            // Opcional: Forzar un poco hacia abajo para mejor adherencia a pendientes
            // _rb.AddForce(gravityNormal * 1f); // Peque�a fuerza constante hacia el suelo
        }

        // 6. Limitar la velocidad m�xima de movimiento perpendicular (horizontal)
        // Esto es importante si se usa AddForce
        float perpSpeed = Vector2.Dot(_rb.linearVelocity, perpendicularDirection);
        if (Mathf.Abs(perpSpeed) > _maxMoveSpeed)
        {
            _rb.linearVelocity = perpendicularDirection * Mathf.Sign(perpSpeed) * _maxMoveSpeed + currentParallelVelocity;
        }
    }

    // Obtiene un vector normalizado perpendicular al vector dado
    private Vector2 GetPerpendicularDirection(Vector2 v)
    {
        // Rotar 90 grados (o -90, depende de la convenci�n deseada)
        return new Vector2(-v.y, v.x).normalized;
    }


    private void CheckGrounded()
    {
        Vector2 origin = (Vector2)_collider.bounds.center + _groundCheckOffset;
        Vector2 size = Vector2.Scale(_collider.bounds.size, _groundCheckBoxSizeMultiplier);
        Vector2 direction = _gravityDirection.normalized; // Direcci�n hacia donde chequear

        _debugGroundHit = Physics2D.BoxCast(origin, size, _rb.rotation, direction, _groundCheckDistance, _groundLayer);

        _isGrounded = _debugGroundHit.collider != null;

        // Opcional: Ajustar ligeramente la posici�n si est� "casi" tocando el suelo
        // Esto puede ayudar a evitar "bouncing" en superficies planas si la gravedad es fuerte
        // if (_isGrounded && _debugGroundHit.distance > 0 && _debugGroundHit.distance < 0.01f)
        // {
        //     _rb.position += direction * _debugGroundHit.distance;
        // }
    }

    public void Dash(Vector2 direction)
    {
        if (direction == Vector2.zero) return; // No hacer dash si la direcci�n es cero

        _isDashing = true;
        _isGrounded = false; // Considerar no grounded durante el dash
        _dashTimer = _dashDuration;
        _dashDirection = direction.normalized;
        // Interrumpir la velocidad actual e iniciar la del dash
        _rb.linearVelocity = _dashDirection * _dashSpeed;
    }

    public void SetGravityDirection(Vector2 gravityDir)
    {
        _gravityDirection = gravityDir.normalized;
        // Podr�as necesitar ajustar la rotaci�n o constraints si la gravedad cambia dr�sticamente
    }

    private void OnDrawGizmosSelected() // Cambiado a Selected para no saturar la vista
    {
        if (_collider == null) return;

        // --- Ground Check BoxCast ---
        Vector3 center = (Vector2)_collider.bounds.center + _groundCheckOffset;
        Vector3 size = Vector2.Scale(_collider.bounds.size, _groundCheckBoxSizeMultiplier);
        Vector3 direction = _gravityDirection.normalized;

        Vector3 groundEnd = center + (direction * _groundCheckDistance);

        Gizmos.color = _isGrounded ? Color.green : Color.yellow;

        // Dibujar el BoxCast requiere m�s l�gica para rotaci�n, esto es una aproximaci�n
        // Guarda la matriz actual
        Matrix4x4 oldMatrix = Gizmos.matrix;
        // Aplica rotaci�n y posici�n
        Gizmos.matrix = Matrix4x4.TRS(groundEnd, transform.rotation, Vector3.one);
        // Dibuja el cubo en el espacio local transformado
        Gizmos.DrawWireCube(Vector3.zero, size);
        // Restaura la matriz original
        Gizmos.matrix = oldMatrix;

        // L�nea de direcci�n de gravedad
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(center, center + direction * 0.5f);

        // L�nea de direcci�n perpendicular
        Gizmos.color = Color.magenta;
        Vector3 perpDir = GetPerpendicularDirection(direction);
        Gizmos.DrawLine(center, center + perpDir * 0.5f);
    }
}