using UnityEngine;

public class Enemy2 : MonoBehaviour
{
    public float walkSpeed = 3f;
    [SerializeField] private float chaseSpeed = 4f; 
    [SerializeField] private float chaseRadius = 4f;
    [SerializeField] private LayerMask playerLayer; 
    [SerializeField] private float patrolDistance = 5f;
    [SerializeField] private bool showDetectionRadius = true; 
    Animator animator; 

    private Rigidbody2D rb;
    private Transform playerTransform; 
    private Vector2 walkDirectionVector;
    public DetectionZone attackZone;
    public enum WalkableDirection { Right, Left }
    private WalkableDirection _walkDirection;

    private enum State { Patrol, Chase, Attack }
    private State currentState = State.Patrol;
    private Vector2 patrolStartPosition; 
    private float timeSinceLastSwitch; 

    public WalkableDirection WalkDirection
    {
        get { return _walkDirection; }
        set
        {
            if (_walkDirection != value)
            {
                gameObject.transform.localScale = new Vector2(gameObject.transform.localScale.x * -1, gameObject.transform.localScale.y);
                walkDirectionVector = value == WalkableDirection.Right ? Vector2.right : Vector2.left;
                patrolStartPosition = transform.position;
            }
            _walkDirection = value;
        }
    }

    public bool _hasTarget = false; 

    public bool HasTarget
    {
        get { return _hasTarget; }
        set
        {
            _hasTarget = value;
            animator.SetBool("hasTarget", value);
        }
    }

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        walkDirectionVector = Vector2.right; 
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        WalkDirection = WalkableDirection.Right; 
        patrolStartPosition = transform.position;
        timeSinceLastSwitch = 0f;
    }

    private void Update()
    {
        HasTarget = attackZone.detectedColliders.Count > 0; 
    }

    public bool CanMove
    {
        get
        {
            return animator.GetBool("canMove");
        }
    }

    private void FixedUpdate()
    {
        
        if (HasTarget && attackZone.detectedColliders.Count > 0)
        {
            currentState = State.Attack;
            playerTransform = attackZone.detectedColliders[0].transform; 
        }
        else
        {
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
        float distanceTraveled = Vector2.Distance(transform.position, patrolStartPosition);

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

        rb.linearVelocity = Vector2.zero;

        Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
        WalkDirection = directionToPlayer.x > 0 ? WalkableDirection.Right : WalkableDirection.Left;

    }

    private void FlipDirection()
    {
        WalkDirection = WalkDirection == WalkableDirection.Right ? WalkableDirection.Left : WalkableDirection.Right;
    }

    private void OnDrawGizmos()
    {
        if (showDetectionRadius)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, chaseRadius);
        }
    }
}
