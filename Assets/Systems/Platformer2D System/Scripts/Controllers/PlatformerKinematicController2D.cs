using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlatformerKinematicController2D : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private Vector2 _gravityDirection = Vector2.down;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _groundCheckDistance = 0.1f;
    [SerializeField] private float _moveSpeed = 5f;
    [SerializeField] private float _dashSpeed = 10f;
    [SerializeField] private float _dashDuration = 0.2f;

    private Rigidbody2D _rb;
    private Collider2D _collider;

    private Vector2 _velocity;
    private Vector2 _inputDirection;
    private bool _isGrounded;
    private bool _isDashing;
    private float _dashTimer;
    private Vector2 _delta;

    public Vector2 InputDirection
    {
        get => _inputDirection;
        set => _inputDirection = Vector2.ClampMagnitude(value, 1f);
    }

    public bool IsGrounded => _isGrounded;
    public bool IsDashing => _isDashing;
    public Vector2 Velocity => _velocity;
    public Vector2 Delta => _delta;
    public Vector2 Gravity => _gravityDirection;

    // Debug
    private RaycastHit2D _debugHitX;
    private RaycastHit2D _debugHitY;
    private RaycastHit2D _debugGroundHit;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.bodyType = RigidbodyType2D.Kinematic;
        _collider = GetComponent<Collider2D>();
    }

    private void Update()
    {
        CheckGrounded();

        if (_isDashing)
        {
            _dashTimer -= Time.deltaTime;
            if (_dashTimer <= 0f)
            {
                _isDashing = false;
            }
        }
    }

    private void FixedUpdate()
    {
        // Si el personaje está tocando el suelo, necesitamos ajustar la velocidad vertical
        if (_isGrounded && !_isDashing)
        {
            // Solo aplicamos gravedad si no estamos en contacto directo con el suelo
            // o si estamos intentando movernos en dirección opuesta a la gravedad
            float dotProduct = Vector2.Dot(_inputDirection, -_gravityDirection);
            if (dotProduct > 0)
            {
                // Si intentamos movernos en contra de la gravedad, permitimos el movimiento normal
                _velocity = _inputDirection * _moveSpeed;
            }
            else
            {
                // Si no, solo permitimos movimiento perpendicular a la gravedad
                Vector2 perpendicularDirection = Vector2.Perpendicular(_gravityDirection).normalized;
                float inputAlongPerp = Vector2.Dot(_inputDirection, perpendicularDirection);
                _velocity = perpendicularDirection * inputAlongPerp * _moveSpeed;
            }
        }
        else if (_isDashing)
        {
            _velocity = _inputDirection.normalized * _dashSpeed;
        }
        else
        {
            // Si no estamos en el suelo y no estamos haciendo dash, aplicamos gravedad
            _velocity = _inputDirection * _moveSpeed + _gravityDirection;
        }

        _delta = _velocity * Time.fixedDeltaTime;
        Move(_delta);
    }

    private void Move(Vector2 delta)
    {
        Vector2 pos = _rb.position;

        // Movimiento X
        if (delta.x != 0 && CanMove(Vector2.right * Mathf.Sign(delta.x), Mathf.Abs(delta.x)))
        {
            pos.x += delta.x;
        }

        // Movimiento Y
        if (delta.y != 0 && CanMove(Vector2.up * Mathf.Sign(delta.y), Mathf.Abs(delta.y)))
        {
            pos.y += delta.y;
        }

        _rb.MovePosition(pos);
    }



    private bool CanMove(Vector2 direction, float distance)
    {
        Vector2 boxCastOrigin = _collider.bounds.center + new Vector3(direction.y * 0.1f, Mathf.Abs(direction.x) * 0.1f, 0);
        RaycastHit2D hit = Physics2D.BoxCast(boxCastOrigin, _collider.bounds.size, 0f, direction, distance, _groundLayer);
        return hit.collider == null || hit.distance > 0f;
    }



    private void CheckGrounded()
    {
        Vector2 origin = (Vector2)_collider.bounds.center;
        Vector2 direction = -_gravityDirection.normalized;
        float distance = _groundCheckDistance;

        _debugGroundHit = Physics2D.BoxCast(origin, _collider.bounds.size * 0.95f, 0f, direction, distance, _groundLayer);
        _isGrounded = _debugGroundHit.collider != null;

        // Si estamos en el suelo, anulamos cualquier componente de velocidad en la dirección de la gravedad
        if (_isGrounded && !_isDashing)
        {
            // Eliminamos la componente de velocidad en la dirección de la gravedad
            float gravityComponent = Vector2.Dot(_velocity, _gravityDirection);
            if (gravityComponent > 0)
            {
                _velocity -= _gravityDirection * gravityComponent;
            }
        }
    }

    public void Dash(Vector2 direction)
    {
        _isDashing = true;
        _dashTimer = _dashDuration;
        _inputDirection = direction.normalized;
    }

    public void SetGravityDirection(Vector2 gravity)
    {
        _gravityDirection = gravity.normalized;
    }

    private void OnDrawGizmos()
    {
        if (_collider == null) return;

        // Dimensiones del BoxCast
        Vector3 size = _collider.bounds.size;
        Vector3 center = _collider.bounds.center;

        // --- Horizontal BoxCast ---
        Vector2 dirX = _inputDirection.x != 0 ? Vector2.right * Mathf.Sign(_inputDirection.x) : Vector2.right;
        Vector3 endX = center + new Vector3(_delta.x, 0f);

        Gizmos.color = _debugHitX.collider != null ? Color.red : new Color(1f, 0.5f, 0.5f, 0.5f);
        Gizmos.DrawWireCube(endX, size);

        // --- Vertical BoxCast ---
        Vector2 dirY = _inputDirection.y != 0 ? Vector2.up * Mathf.Sign(_inputDirection.y) : Vector2.up;
        Vector3 endY = center + new Vector3(0f, _delta.y);

        Gizmos.color = _debugHitY.collider != null ? Color.blue : new Color(0.5f, 0.5f, 1f, 0.5f);
        Gizmos.DrawWireCube(endY, size);

        // --- Ground Check BoxCast ---
        Vector2 gravityDir = -_gravityDirection.normalized;
        Vector3 groundEnd = center + (Vector3)(gravityDir * _groundCheckDistance);

        Gizmos.color = _debugGroundHit.collider != null ? Color.green : Color.yellow;
        Gizmos.DrawWireCube(groundEnd, size);

    }

}
