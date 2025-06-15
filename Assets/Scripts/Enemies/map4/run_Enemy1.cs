using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class run_Enemy1 : MonoBehaviour
{
    public float walkSpeed = 3f;
    [SerializeField] private float chaseSpeed = 4f; // Tốc độ đuổi theo
    [SerializeField] private float chaseRadius = 8f; // Bán kính phát hiện player
    [SerializeField] private LayerMask playerLayer; // Layer của player
    [SerializeField] private float patrolDistance = 5f; // Khoảng cách di chuyển tuần tra
    [SerializeField] private bool showDetectionRadius = true; // Bật/tắt hiển thị vòng tròn
    Animator animator; // Animator để điều khiển hoạt ảnh

    private Rigidbody2D rb;
    private Transform playerTransform; // Vị trí của player
    private Vector2 walkDirectionVector;
    public DetectionZone attackZone;
    public enum WalkableDirection { Right, Left } // Hướng tuần tra
    private WalkableDirection _walkDirection;

    private enum State { Patrol, Chase, Attack } // Thêm trạng thái Attack
    private State currentState = State.Patrol;
    private Vector2 patrolStartPosition; // Vị trí bắt đầu của đoạn di chuyển tuần tra
    private float timeSinceLastSwitch; // Theo dõi thời gian chuyển hướng tuần tra

    public WalkableDirection WalkDirection
    {
        get { return _walkDirection; }
        set
        {
            if (_walkDirection != value)
            {
                gameObject.transform.localScale = new Vector2(gameObject.transform.localScale.x * -1, gameObject.transform.localScale.y);
                walkDirectionVector = value == WalkableDirection.Right ? Vector2.right : Vector2.left;
                patrolStartPosition = transform.position; // Cập nhật vị trí bắt đầu khi đổi hướng
            }
            _walkDirection = value;
        }
    }

    public bool _hasTarget = false; // Biến riêng để theo dõi trạng thái có mục tiêu

    public bool HasTarget
    {
        get { return _hasTarget; }
        set
        {
            _hasTarget = value;
            animator.SetBool("hasTarget", value); // Cập nhật animator
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        WalkDirection = WalkableDirection.Right; // Khởi tạo hướng tuần tra
        patrolStartPosition = transform.position; // Lưu vị trí ban đầu
        timeSinceLastSwitch = 0f;
    }

    private void Update()
    {
        HasTarget = attackZone.detectedColliders.Count > 0; // Cập nhật trạng thái có mục tiêu từ DetectionZone
    }

    public bool CanMove
    {
        get
        {
            return animator.GetBool("CanMove");
        }
    }

    private void FixedUpdate()
    {
        // Kiểm tra trạng thái tấn công trước
        if (HasTarget)
        {
            currentState = State.Attack;
            playerTransform = attackZone.detectedColliders[0].transform; // Lấy transform của player trong vùng tấn công
        }
        else
        {
            // Phát hiện player trong chaseRadius
            Collider2D playerCollider = Physics2D.OverlapCircle(transform.position, chaseRadius, playerLayer);
            if (playerCollider != null)
            {
                currentState = State.Chase;
                playerTransform = playerCollider.transform;
            }
            else
            {
                currentState = State.Patrol;
                playerTransform = null;
            }
        }

        // Xử lý theo trạng thái
        switch (currentState)
        {
            case State.Patrol:
                HandlePatrol();
                break;
            case State.Chase:
                HandleChase();
                break;
            case State.Attack:
                HandleAttack();
                break;
        }
    }

    private void HandlePatrol()
    {
        // Tính khoảng cách di chuyển từ patrolStartPosition
        float distanceTraveled = Vector2.Distance(transform.position, patrolStartPosition);

        // Chuyển hướng khi đạt patrolDistance
        if (distanceTraveled >= patrolDistance)
        {
            FlipDirection();
        }

        if (CanMove)
        {
            rb.linearVelocity = new Vector2(walkSpeed * walkDirectionVector.x, rb.linearVelocity.y);
        }
    }

    private void HandleChase()
    {
        if (playerTransform == null) return;

        // Tính hướng đến player
        Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
        WalkDirection = directionToPlayer.x > 0 ? WalkableDirection.Right : WalkableDirection.Left;

        if (CanMove)
        {
            rb.linearVelocity = new Vector2(directionToPlayer.x * chaseSpeed, rb.linearVelocity.y);
        }
        else
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }

    private void HandleAttack()
    {
        if (playerTransform == null) return;

        // Dừng hoàn toàn di chuyển
        rb.linearVelocity = Vector2.zero;

        // Xác định hướng để quay mặt về phía player
        Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
        WalkDirection = directionToPlayer.x > 0 ? WalkableDirection.Right : WalkableDirection.Left;

        // Animation tấn công được điều khiển bởi hasTarget = true (không dùng trigger)
    }

    private void FlipDirection()
    {
        WalkDirection = WalkDirection == WalkableDirection.Right ? WalkableDirection.Left : WalkableDirection.Right;
    }

    private void OnDrawGizmos()
    {
        // Vẽ bán kính phát hiện player chỉ khi showDetectionRadius được bật
        if (showDetectionRadius)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, chaseRadius);
        }
    }
}