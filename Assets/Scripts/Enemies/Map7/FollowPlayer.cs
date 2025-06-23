using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public Transform player;
    public float speed = 2f;
    public float yOffset = 0.5f;

    public float detectDistance = 10f;
    public float stopDistance = 0.5f;

    private bool facingRight = true;
    private Animator animator;

    void Start()
    {
        // Lấy component Animator
        animator = GetComponent<Animator>();
    }

    void FixedUpdate()
    {
        // Không di chuyển nếu đang tấn công
        if (animator.GetBool("Attacking"))
        {
            animator.SetBool("isRunning", false); // Tắt animation chạy nếu đang tấn công
            return;
        }

        Vector3 targetPosition = player.position + new Vector3(0, yOffset, 0);
        float distanceToPlayer = Vector3.Distance(transform.position, targetPosition);

        if (distanceToPlayer <= detectDistance)
        {
            if (distanceToPlayer > stopDistance)
            {
                animator.SetBool("isRunning", true);

                Vector3 direction = (targetPosition - transform.position).normalized;
                transform.position += direction * speed * Time.fixedDeltaTime;

                HandleFlip(direction.x);
            }
            else
            {
                animator.SetBool("isRunning", false);
            }
        }
        else
        {
            animator.SetBool("isRunning", false);
        }
    }

    void HandleFlip(float moveDirectionX)
    {
        if (moveDirectionX > 0 && !facingRight)
        {
            Flip();
        }
        else if (moveDirectionX < 0 && facingRight)
        {
            Flip();
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 localScale = transform.localScale;
        localScale.x *= -1;
        transform.localScale = localScale;
    }
}
