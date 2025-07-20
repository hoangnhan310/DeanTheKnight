using UnityEngine;

public class PatrolEnemy : MonoBehaviour
{
    public float speed = 2f;
    public Transform checkPoint;
    public float distance = 1f;
    public LayerMask layerMask;
    private bool facingLeft = true;
    public bool inRange = false;
    public Transform player;
    public float attackRange = 10f;
    public float retrieveDistance = 2.5f;
    public float chaseSpeed = 3f;
    public Animator animator;
    public Transform attackPoint;
    public float attackRadius = 1f;
    public LayerMask attackLayer;

    void Update()
    {
        if (Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            inRange = true;
        }
        else
        {
            inRange = false;
        }
        if (inRange)
        {
            if (player.position.x > transform.position.x && facingLeft == true)
            {
                transform.eulerAngles = new Vector3(0, -180, 0);
                facingLeft = false;
            }
            else if (player.position.x < transform.position.x && facingLeft == false)
            {
                transform.eulerAngles = new Vector3(0, 0, 0);
                facingLeft = true;
            }
            if (Vector2.Distance(transform.position, player.position) > retrieveDistance)
            {
                animator.SetBool("Attack1", false);
                transform.position = Vector2.MoveTowards(transform.position, player.position, chaseSpeed * Time.deltaTime);

            }
            else
            {
                animator.SetBool("Attack1", true);
            }
        }
        else
        {
            transform.Translate(Vector2.left * speed * Time.deltaTime);
            RaycastHit2D hit = Physics2D.Raycast(checkPoint.position, Vector2.down, distance, layerMask);

            if (hit == false && facingLeft)
            {
                transform.eulerAngles = new Vector3(0, -180, 0);
                facingLeft = false;
            }
            else if (hit == false && facingLeft == false)
            {
                transform.eulerAngles = new Vector3(0, 0, 0);
                facingLeft = true;
            }
        }

    }

    public void Attack()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, attackRadius, attackLayer);

        foreach (Collider2D collider in colliders)
        {
            Debug.Log("Hit: " + collider.name);
        }
    }
    private void OnDrawGizmosSelected()
    {
        if (checkPoint == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(checkPoint.position, Vector2.down * distance);
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.red;

        Gizmos.DrawWireSphere(attackPoint.position, attackRange);

        if (attackPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
    }
}
