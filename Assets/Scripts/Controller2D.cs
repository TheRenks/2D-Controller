using UnityEngine;

public class Controller2D : MonoBehaviour
{
    private const float MinMoveDistance = 1E-4f;
    private const float _contactOffset = 1E-2f;

    [SerializeField] private Vector2 _offset = Vector2.zero;
    [SerializeField][Min(0.03125f)] private Vector2 _size = Vector2.one;

    public Vector2 Offset
    {
        get
        {
            return _offset;
        }
        set
        {
            _offset = value;
            UpdateColliderPropreties();
        }
    }

    public Vector2 Size
    {
        get
        {
            return _size;
        }
        set
        {
            _size = value;
            UpdateColliderPropreties();
        }
    }

    private BoxCollider2D _collider = null;
    private RaycastHit2D[] results = new RaycastHit2D[5];
    private ContactInfo _contact;
    public ContactInfo GetContact() => _contact;

    private void Awake()
    {
        if (TryGetComponent(out Collider2D collider)) Destroy(collider);
        SetupCollider();
        UpdateColliderPropreties();
    }

    private void SetupCollider()
    {
        _collider = gameObject.AddComponent<BoxCollider2D>();
        _collider.hideFlags = HideFlags.HideInInspector;
    }

    private void UpdateColliderPropreties()
    {
        if (_collider == null) return;

        _collider.offset = _offset;
        _collider.size = _size;
    }

    public void Move(Vector2 movement)
    {
        var position = (Vector2)transform.position;

        _contact.Reset();
        HandleCollision(ref position, Vector2.right * movement);
        HandleCollision(ref position, Vector2.up * movement);

        transform.position = position;
    }

    private void HandleCollision(ref Vector2 position, Vector2 movement)
    {
        var direction = movement.normalized;
        var distance = movement.magnitude;

        if (distance < MinMoveDistance) return;

        if (Cast(position, direction, distance, out RaycastHit2D hitInfo))
        {
            position += hitInfo.distance * direction;

            _contact.Update(direction, hitInfo);

            return;
        }

        position += movement;
    }

    private bool Cast(Vector2 position, Vector2 direction, float distance, out RaycastHit2D hitInfo)
    {
        var safeDistance = 8.0f * _contactOffset;
        var sizeCorrect = Vector2.one * (_contactOffset * 3.0f);
        var count = Physics2D.BoxCastNonAlloc(position + (Vector2)_offset, (Vector2)_size - sizeCorrect, 0.0f, direction, results, distance + safeDistance, GetCollisionMask());

        hitInfo = GetClosestHit(count, results);

        if (hitInfo)
        {
            var dot = Vector2.Dot(direction, hitInfo.normal);

            if (dot >= 0.0f)
            {
                hitInfo.distance -= safeDistance;
            }
            else
            {
                hitInfo.distance -= Mathf.Min(-_contactOffset / dot, safeDistance);
            }

            if (hitInfo.distance >= distance) return false;

            if (hitInfo.distance < 0.0f) hitInfo.distance = 0.0f;

            return true;
        }

        return false;
    }

    private RaycastHit2D GetClosestHit(int count, RaycastHit2D[] hits)
    {
        int closestHitIndex = -1;
        float closestHitDistance = float.MaxValue;

        for (int i = 0; i < count; i++)
        {
            var currentHit = results[i];

            // Ignore your own collider and trigger colliders
            if (currentHit.collider.isTrigger || currentHit.distance <= 0.0f) continue;

            if (currentHit.distance < closestHitDistance)
            {
                closestHitDistance = currentHit.distance;
                closestHitIndex = i;
            }
        }

        return closestHitIndex < 0 ? default : results[closestHitIndex];
    }

    private LayerMask GetCollisionMask()
    {
        return Physics2D.GetLayerCollisionMask(gameObject.layer);
    }

    public struct ContactInfo
    {
        public bool Below;
        public bool Above;
        public bool Left;
        public bool Right;
        private RaycastHit2D[] _hits;
        public RaycastHit2D[] Hits { get => _hits; set => _hits = value; }

        public void Reset()
        {
            Below = Above = false;
            Left = Right = false;
            _hits = new RaycastHit2D[2];
        }

        public void Update(Vector2 hitDirection, RaycastHit2D hitInfo)
        {
            var direction = Vector2.Dot(hitDirection, Vector2.up);

            if (direction == 0)
            {
                Left = hitDirection == Vector2.left;
                Right = hitDirection == Vector2.right;
                _hits[0] = hitInfo;
            }
            else
            {
                Below = hitDirection == Vector2.down;
                Above = hitDirection == Vector2.up;
                _hits[1] = hitInfo;
            }
        }
    }

    private readonly Color defaultColliderColor = new Color(0.568f, 0.956f, 0.545f, 0.752f);
    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying) return;

        Gizmos.color = defaultColliderColor;
        Gizmos.DrawWireCube((Vector2)transform.position + _offset, _size);
    }
}