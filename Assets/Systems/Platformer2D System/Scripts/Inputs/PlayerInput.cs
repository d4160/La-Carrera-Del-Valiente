using UnityEngine;

[RequireComponent(typeof(PlatformerDynamicController2D))]
public class PlayerInput : MonoBehaviour
{
    private PlatformerDynamicController2D controller;

    // Puedes a�adir aqu� referencias a otros sistemas si el input los afecta (UI, inventario, etc.)

    void Awake()
    {
        controller = GetComponent<PlatformerDynamicController2D>();
        if (controller == null)
        {
            Debug.LogError("PlatformerDynamicController2D not found on this GameObject!", this);
            enabled = false; // Deshabilitar este script si no encuentra el controlador
        }
    }

    void Update()
    {
        if (controller == null) return;

        // --- Movimiento Horizontal ---
        float horizontalInput = Input.GetAxisRaw("Horizontal"); // Usa GetAxisRaw para respuesta inmediata
        controller.SetHorizontalInput(horizontalInput);

        // --- Salto ---
        // Detectar cu�ndo se presiona el bot�n de salto
        if (Input.GetButtonDown("Jump"))
        {
            controller.RequestJump();
        }

        // Detectar cu�ndo se suelta el bot�n de salto (para altura variable)
        if (Input.GetButtonUp("Jump"))
        {
            controller.RequestJumpCancel();
        }

        // --- Dash ---
        if (Input.GetButtonDown("Dash"))
        {
            // Calcular direcci�n del Dash
            // Opci�n 1: Usar input actual (Horizontal/Vertical)
            float dashHorizontal = Input.GetAxisRaw("Horizontal");
            float dashVertical = Input.GetAxisRaw("Vertical");
            Vector2 dashDirection = new Vector2(dashHorizontal, dashVertical).normalized;

            // Opci�n 2: Si no hay input, dash en la direcci�n que mira
            if (dashDirection == Vector2.zero)
            {
                dashDirection = controller.IsFacingRight ? Vector2.right : Vector2.left;
            }

            // Opci�n 3: Dash hacia el cursor del rat�n (requiere convertir pos del rat�n a world space)
            /*
            if (Camera.main != null) {
                Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                dashDirection = (mousePos - transform.position);
                dashDirection.z = 0; // Asegurar que es 2D
                dashDirection.Normalize();
            } else {
                 // Fallback si no hay c�mara principal
                 dashDirection = controller.IsFacingRight ? Vector2.right : Vector2.left;
            }
            */


            controller.RequestDash(dashDirection);
        }

        // --- Otras Acciones (Ejemplos) ---
        /*
        if (Input.GetButtonDown("Fire1")) {
            // Disparar, atacar, etc.
        }
        if (Input.GetButtonDown("Interact")) {
            // Interactuar con objetos
        }
        */
    }
}