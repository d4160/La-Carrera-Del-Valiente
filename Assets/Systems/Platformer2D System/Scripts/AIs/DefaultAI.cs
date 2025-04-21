using UnityEngine;
using System.Collections; // Necesario para Coroutines

[RequireComponent(typeof(PlatformerDynamicController2D))]
public class DefaultAI : MonoBehaviour
{
    [Header("AI Settings")]
    [SerializeField] private Transform playerTarget; // Asigna el Transform del jugador aqu�
    [SerializeField] private float detectionRange = 15f; // Rango para empezar a perseguir
    [SerializeField] private float stoppingDistance = 1.5f; // Distancia a la que deja de acercarse horizontalmente
    [SerializeField] private float jumpObstacleCheckDistance = 0.6f; // Distancia para detectar muros enfrente
    [SerializeField] private float ledgeCheckDistance = 0.5f; // Distancia hacia abajo y adelante para detectar bordes
    [SerializeField] private float jumpDecisionDelay = 0.5f; // Peque�o retraso antes de intentar saltar un obst�culo
    [SerializeField] private float dashDecisionDelay = 0.3f; // Peque�o retraso antes de decidir hacer dash
    [SerializeField] private float dashRange = 5f; // Distancia m�nima para considerar usar dash para acercarse
    [SerializeField] private LayerMask obstacleLayer; // Capa que contiene muros y obst�culos (normalmente la misma que groundLayer)

    [Header("References")]
    [SerializeField] private PlatformerDynamicController2D controller;
    [SerializeField] private Transform wallCheckPoint; // Punto enfrente de la IA para detectar muros
    [SerializeField] private Transform ledgeCheckPoint; // Punto enfrente y abajo para detectar bordes

    private float timeSinceLastJumpAttempt = Mathf.Infinity;
    private float timeSinceLastDashAttempt = Mathf.Infinity;
    private bool isPlayerInRange = false;
    private bool isFacingObstacle = false;
    private bool isNearLedge = false;


    void Awake()
    {
        if (controller == null) controller = GetComponent<PlatformerDynamicController2D>();
        if (playerTarget == null)
        {
            // Intentar encontrar al jugador por Tag si no est� asignado
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) playerTarget = playerObj.transform;
            else Debug.LogWarning("AIController: Player target not set and couldn't find GameObject with tag 'Player'.", this);
        }

        // Configurar puntos de chequeo si no est�n asignados (requiere que el objeto tenga un Collider)
        SetupCheckPoints();
    }

    void Update()
    {
        if (playerTarget == null || controller == null) return; // Salir si falta algo esencial

        // Actualizar timers
        timeSinceLastJumpAttempt += Time.deltaTime;
        timeSinceLastDashAttempt += Time.deltaTime;

        // Comprobar percepci�n del entorno
        CheckEnvironment();

        // L�gica de decisi�n principal
        DecideAction();
    }

    private void SetupCheckPoints()
    {
        Collider2D col = GetComponent<Collider2D>();
        if (col == null)
        {
            Debug.LogError("AIController requires a Collider2D to setup check points automatically.", this);
            return;
        }
        float colliderWidth = col.bounds.extents.x;
        float colliderHeight = col.bounds.extents.y;

        if (wallCheckPoint == null)
        {
            Transform found = transform.Find("WallCheck");
            if (found) wallCheckPoint = found;
            else
            {
                GameObject wc = new GameObject("WallCheck");
                wc.transform.SetParent(transform);
                // Posicionar ligeramente por delante y a media altura
                wc.transform.localPosition = new Vector3(colliderWidth + 0.1f, 0, 0);
                wallCheckPoint = wc.transform;
            }
        }
        if (ledgeCheckPoint == null)
        {
            Transform found = transform.Find("LedgeCheck");
            if (found) ledgeCheckPoint = found;
            else
            {
                GameObject lc = new GameObject("LedgeCheck");
                lc.transform.SetParent(transform);
                // Posicionar ligeramente por delante y justo debajo del nivel del suelo
                lc.transform.localPosition = new Vector3(colliderWidth + 0.1f, -colliderHeight - 0.1f, 0);
                ledgeCheckPoint = lc.transform;
            }
        }
    }


    private void CheckEnvironment()
    {
        // �Est� el jugador dentro del rango?
        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);
        isPlayerInRange = distanceToPlayer <= detectionRange;

        if (!isPlayerInRange) return; // No hacer m�s chequeos si el jugador est� lejos

        // Chequeo de Muro (direcci�n en la que mira la IA)
        float checkDirection = controller.IsFacingRight ? 1f : -1f;
        Vector2 wallCheckOrigin = wallCheckPoint != null ? (Vector2)wallCheckPoint.position : (Vector2)transform.position + new Vector2(GetComponent<Collider2D>().bounds.extents.x * checkDirection, 0);
        isFacingObstacle = Physics2D.Raycast(wallCheckOrigin, Vector2.right * checkDirection, jumpObstacleCheckDistance, obstacleLayer);
        Debug.DrawRay(wallCheckOrigin, Vector2.right * checkDirection * jumpObstacleCheckDistance, isFacingObstacle ? Color.red : Color.yellow);


        // Chequeo de Borde (direcci�n en la que mira la IA)
        Vector2 ledgeCheckOrigin = ledgeCheckPoint != null ? (Vector2)ledgeCheckPoint.position : (Vector2)transform.position + new Vector2((GetComponent<Collider2D>().bounds.extents.x + 0.1f) * checkDirection, -GetComponent<Collider2D>().bounds.extents.y - 0.1f);
        isNearLedge = !Physics2D.Raycast(ledgeCheckOrigin, Vector2.down, ledgeCheckDistance, obstacleLayer); // No hay suelo debajo
        Debug.DrawRay(ledgeCheckOrigin, Vector2.down * ledgeCheckDistance, isNearLedge ? Color.blue : Color.cyan);
    }

    private void DecideAction()
    {
        if (!isPlayerInRange || playerTarget == null)
        {
            controller.SetHorizontalInput(0); // Detenerse si el jugador est� fuera de rango
            return;
        }

        Vector2 directionToPlayer = (playerTarget.position - transform.position);
        float horizontalDistance = Mathf.Abs(directionToPlayer.x);

        // --- Decisiones basadas en obst�culos y distancia ---

        // 1. Si hay un obst�culo enfrente y estoy en el suelo: Intenta Saltar
        if (isFacingObstacle && controller.IsGrounded && timeSinceLastJumpAttempt > jumpDecisionDelay)
        {
            //Debug.Log("AI: Obstacle detected, attempting jump.");
            controller.RequestJump();
            timeSinceLastJumpAttempt = 0f; // Reiniciar timer
            // Podr�amos a�adir l�gica para saltar m�s alto o hacer dash despu�s de saltar si el obst�culo es alto
            return; // Prioridad alta, no hacer otras acciones de movimiento este frame
        }

        // 2. Si estoy cerca de un borde y el jugador est� al otro lado: Intenta hacer Dash (si es necesario)
        if (isNearLedge && controller.IsGrounded && timeSinceLastDashAttempt > dashDecisionDelay)
        {
            // Solo hacer dash si el jugador est� razonablemente lejos y en la direcci�n del borde
            if (Mathf.Sign(directionToPlayer.x) == Mathf.Sign(controller.IsFacingRight ? 1 : -1) && horizontalDistance > stoppingDistance + 0.5f)
            {
                //Debug.Log("AI: Ledge detected, attempting dash across.");
                Vector2 dashDir = directionToPlayer.normalized;
                // Opcional: Forzar un dash m�s horizontal si es un hueco
                // dashDir = new Vector2(Mathf.Sign(directionToPlayer.x), 0.1f).normalized;
                controller.RequestDash(dashDir);
                timeSinceLastDashAttempt = 0f;
                return;
            }
        }

        // 3. Si estoy en el aire y el jugador est� lejos o en posici�n dif�cil: Intenta hacer Dash
        if (!controller.IsGrounded && timeSinceLastDashAttempt > dashDecisionDelay && horizontalDistance > dashRange)
        {
            //Debug.Log("AI: In air, attempting dash towards player.");
            controller.RequestDash(directionToPlayer.normalized);
            timeSinceLastDashAttempt = 0f;
            return;
        }


        // 4. Movimiento Horizontal B�sico de Persecuci�n
        if (horizontalDistance > stoppingDistance)
        {
            // Moverse hacia el jugador
            float moveDirection = Mathf.Sign(directionToPlayer.x);
            controller.SetHorizontalInput(moveDirection);
        }
        else
        {
            // Detenerse si est� lo suficientemente cerca horizontalmente
            controller.SetHorizontalInput(0);

            // Considerar saltar si el jugador est� por encima
            if (directionToPlayer.y > 1.0f && controller.IsGrounded && timeSinceLastJumpAttempt > jumpDecisionDelay)
            {
                //Debug.Log("AI: Player above, attempting jump.");
                controller.RequestJump();
                timeSinceLastJumpAttempt = 0f;
            }
        }
    }

    // Gizmos para visualizar rangos y puntos de chequeo
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, stoppingDistance);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, dashRange);

        // Dibuja los puntos de chequeo si existen
        if (wallCheckPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawSphere(wallCheckPoint.position, 0.1f);
        }
        if (ledgeCheckPoint != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(ledgeCheckPoint.position, 0.1f);
        }
    }
}