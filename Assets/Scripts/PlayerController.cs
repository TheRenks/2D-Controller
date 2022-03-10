using UnityEngine;

public class PlayerController : MonoBehaviour
{
    private Controller2D _controller = null;
    private Vector2 _globalGravity => Physics2D.gravity;

    [Header("Physics")]
    [SerializeField] private Vector2 _velocity = Vector2.zero;
    [SerializeField] private float _gravityScale = 1.0f;
    [SerializeField] private float _drag = 10.0f;

    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 100.0f;
    [SerializeField] private float _currentSpeed = 0.0f;

    [Header("Crouch")]
    public float _crouchHeight = 1.0f;
    private float _defaultHeight = 0.0f;

    [Header("Jump")]
    [SerializeField] private float _maxFallSpeed = -20.0f;
    [SerializeField] private float _jumpHeight = 3.5f;
    [SerializeField] private float _jumpCutForce = 2.0f;
    [SerializeField] private float _jumpBuffer = 0.2f;
    [SerializeField] private float _coyoteTime = 0.1f;
    private float _jumpBufferTimer = 0.0f;
    private float _coyoteTimer = 0.0f;

    [Header("Bools")]
    public bool IsGrounded = false;
    public bool IsJumping = false;
    public bool Jumped = false;
    public bool IsCrouched = false;
    [HideInInspector] public bool WasGrounded;
    [HideInInspector] public bool Landed;

    [Header("Audio")]
    [SerializeField] private AudioSource _splatSound;

    private void Awake()
    {
        _controller = GetComponent<Controller2D>();
    }

    private void Start()
    {
        _defaultHeight = _controller.Size.y;
    }

    private void Update()
    {
        var direction = GetInput();
        GroundChecks();
        HandleHorizontalMovement(direction);
        HandleJump();
        HandlePhysics();
        ChangeHeight(IsCrouched ? _crouchHeight : _defaultHeight);
        HandleAudios();
        _controller.Move(_velocity * Time.deltaTime);
        CancelVelocity();
    }

    private void HandleHorizontalMovement(Vector2 direction)
    {
        IsCrouched = direction.y < 0.0f && IsGrounded;
        CheckCeiling();

        var targetSpeed = IsCrouched ? 20.0f : _moveSpeed;

        _currentSpeed = !IsCrouched ? _moveSpeed : Mathf.Lerp(_currentSpeed, targetSpeed, 0.5f * Time.deltaTime);

        _velocity.x += direction.x * _currentSpeed * Time.deltaTime;
    }

    private void CheckCeiling()
    {
        var overlaps = Physics2D.OverlapBoxAll((Vector2)transform.position + Vector2.up * 0.25f, Vector2.one - Vector2.one * 0.05f, 0.0f);

        for (int i = 0; i < overlaps.Length; i++)
        {
            var collider = overlaps[i];

            TryGetComponent(out Collider2D thisCollider);

            if (collider == thisCollider || collider.isTrigger) continue;

            IsCrouched = true;
            break;
        }
    }

    private void HandlePhysics()
    {
        _velocity.x = CalculateDrag(_velocity.x, _drag);

        _velocity += _globalGravity * _gravityScale * Time.deltaTime;

        if (_velocity.y < _maxFallSpeed) _velocity.y = _maxFallSpeed;
    }

    private void HandleJump()
    {
        Jumped = false;
        if (IsGrounded) IsJumping = false;

        if (Input.GetButtonDown("Jump")) _jumpBufferTimer = Time.time + _jumpBuffer;
        else if (!IsJumping && WasGrounded) _coyoteTimer = Time.time + _coyoteTime;

        if (IsGrounded && _jumpBufferTimer > Time.time || Input.GetButtonDown("Jump") && _coyoteTimer > Time.time)
        {
            Jumped = true;
            IsJumping = true;

            DoJump();

            _jumpBufferTimer = 0.0f;
            _coyoteTimer = 0.0f;
        }

        if (IsJumping && !Input.GetButton("Jump") && _velocity.y > 0.0f)
        {
            _velocity.y -= _jumpCutForce * Time.deltaTime;
        }
    }

    private void DoJump()
    {
        var jumpVelocity = Mathf.Sqrt(-2.0f * _globalGravity.y * _gravityScale * _jumpHeight);
        _velocity.y = jumpVelocity;
    }

    private void GroundChecks()
    {
        var isGroundedThisFrame = IsGrounded;
        var wasGroundedThisFrame = !IsGrounded;

        IsGrounded = _controller.GetContact().Below;

        WasGrounded = isGroundedThisFrame && !IsGrounded;
        Landed = wasGroundedThisFrame && IsGrounded;
    }

    private void CancelVelocity()
    {
        for (int i = 0; i < _controller.GetContact().Hits.Length; i++)
        {
            var hitInfo = _controller.GetContact().Hits[i];

            _velocity = Projection(_velocity, hitInfo.normal);
        }
    }

    private Vector2 Projection(Vector2 vector, Vector2 normal)
    {
        var dot = Vector2.Dot(vector, normal);

        if (dot > float.Epsilon) return vector;

        return vector - dot * normal;
    }

    private float CalculateDrag(float velocity, float drag)
    {
        if (Mathf.Abs(velocity) < 1E-4f) return 0.0f;

        return velocity /= 1.0f + (drag * Time.deltaTime);
    }

    private void ChangeHeight(float value)
    {
        _controller.Offset = new Vector2(_controller.Offset.x, (value - 1.0f) * 0.5f);
        _controller.Size = new Vector2(_controller.Size.x, value);
    }

    public float GetSpeed()
    {
        return Mathf.Abs(_velocity.x);
    }

    public Vector2 GetInput()
    {
        var direction = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        return direction;
    }

    private void HandleAudios()
    {
        if (_splatSound == null) return;

        if (Jumped)
            PlaySound(_splatSound, 2.0f, 3.0f);
    }

    private void PlaySound(AudioSource audio, float min = 1.0f, float max = 1.0f)
    {
        audio.pitch = Random.Range(min, max);
        audio.Play();
    }
}