using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    #region AnimationsHash
    private static readonly int RunSpeed = Animator.StringToHash("RunSpeed");
    private static readonly int Jumped = Animator.StringToHash("Jumped");
    private static readonly int Landed = Animator.StringToHash("Landed");
    private static readonly int IsCrouched = Animator.StringToHash("IsCrouched");
    #endregion
    private PlayerController _player = null;
    private Animator _animator = null;
    private float _value = 0.0f;
    private float _velocity = 0.0f;

    [SerializeField] private ParticleSystem _moveParticle;
    [SerializeField] private ParticleSystem _jumpParticle;
    [SerializeField] private ParticleSystem _landParticle;

    private void Awake()
    {
        _player = GetComponentInParent<PlayerController>();
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (_player == null) return;
        HandleEffects();
        SetAnimationsParameters();

        if (_player.GetInput().x != 0.0f || _player.GetInput().x == 0.0f && !_player.IsGrounded)
        {
            var targetValue = _player.GetInput().x * 6.0f * Mathf.Clamp01(_player.GetSpeed());
            _value = Mathf.MoveTowards(_value, targetValue, 60.0f * Time.deltaTime);
        }
        else if (_player.IsGrounded)
        {
            _value = SpringDamping(_value, ref _velocity, 20.0f, 0.85f);
        }

        var targetRotation = Quaternion.Euler(0.0f, 0.0f, _value);

        _animator.transform.rotation = Quaternion.Lerp(_animator.transform.rotation, targetRotation, 1);
    }

    private void HandleEffects()
    {
        if (_player.Jumped) _jumpParticle.Play();
        if (_player.Landed)
        {
            _landParticle.Play();
        }
        if (_player.GetInput().x != 0.0f && _player.IsGrounded && !_player.IsCrouched)
        {
            if (_moveParticle.isStopped)
                _moveParticle.Play();
        }
        else
        {
            if (_moveParticle.isPlaying)
                _moveParticle.Stop();
        }
    }

    private float SpringDamping(float value, ref float velocity, float strength, float damping)
    {
        var x = velocity - 1.0f;
        var force = -strength * x;
        value += force * Time.deltaTime;
        velocity += value;
        value *= damping;

        if (Mathf.Abs(value) < 1E-4f) value = 0.0f;

        return value;
    }

    private void SetAnimationsParameters()
    {
        _animator.SetFloat(RunSpeed, _player.IsGrounded ? Mathf.Max(1.0f, _player.GetSpeed() * 0.5f) : 1.0f);
        if (_player.Landed)
        {
            _animator.SetTrigger(Landed);
        }
        else if (_player.Jumped) _animator.SetTrigger(Jumped);
        _animator.SetBool(IsCrouched, _player.IsCrouched);
    }
}