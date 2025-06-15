using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    #region Public Variables
    public Transform rayCast;
    public LayerMask rayCastMask;
    public float rayCastLength;
    public float attackDistance; //Minimum distance to attack the player
    public float moveSpeed;
    public float timer; //Time for cooldown between attacks
    #endregion

    #region Private Variables
    private RaycastHit2D hit;
    private GameObject target;
    private Animator animator;
    private float distanceToTarget; //Store the distance b/w enemy and player
    private bool attackMode;
    private bool inRange; //Check if the player is in range
    private bool cooling; //Check if the enemy is cooling down after an attack
    private float intTimer;
    #endregion

    private void Awake()
    {
        intTimer = timer; //Store the initial timer value
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (inRange)
        {
            hit = Physics2D.Raycast(rayCast.position, Vector2.left, rayCastLength, rayCastMask);
            RaycastDebugger();
        }
        //When Player is detected
        if (hit.collider != null)
        {
            EnemyLogic();
        }
        else if (hit.collider == null) 
        { 
            inRange = false;
        }

        if (inRange == false) 
        {
            animator.SetBool("canRun", false);
            StopAttack();
        }
    }

    public void OnTriggerEnter2D(Collider2D trigger)
    {
        if (trigger.gameObject.tag == "Player")
        {
            target = trigger.gameObject;
            inRange = true;
        }
    }

    public void EnemyLogic() 
    { 
        distanceToTarget = Vector2.Distance(transform.position, target.transform.position);
        if (distanceToTarget > attackDistance) 
        {
            Move();
            StopAttack();
        }
        else if (attackDistance >= distanceToTarget && cooling == false)
        {
            Attack();
        }

        if (cooling) 
        {
            Cooldown(); //Reduce the timer value
            animator.SetBool("Attack", false);
        }
    }

    public void Move()
    {
        animator.SetBool("canRun", true); //Set the run animation
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("Attack"))  
        { 
            Vector2 targetPosition = new Vector2(target.transform.position.x, transform.position.y);

            transform.position = Vector2.MoveTowards(transform.position, targetPosition, moveSpeed * Time.deltaTime);
        }
    }

    public void Attack() 
    {
        timer = intTimer; //Reset the timer when Player enter the attack range
        attackMode = true; //To check if Enemy can still attack or not

        animator.SetBool("canRun", false); //Stop the run animation
        animator.SetBool("Attack", true); //Set the attack animation
    }

    public void Cooldown() 
    { 
        timer -= Time.deltaTime; //Reduce the timer value

        if (timer <= 0 && cooling && attackMode) 
        {
            cooling = false; //Stop cooling down
            timer = intTimer; //Reset the timer
        }
    }

    public void StopAttack() 
    {
        cooling = false;
        attackMode = false;

        animator.SetBool("Attack", false); //Stop the attack animation
    }

    public void RaycastDebugger()
    {
        if (distanceToTarget > attackDistance)
        {
            Debug.DrawRay(rayCast.position, Vector2.left * rayCastLength, Color.red);
        }
        else if (distanceToTarget < attackDistance) 
        {
            Debug.DrawRay(rayCast.position, Vector2.left * rayCastLength, Color.green);
        }
    }

    public void TriggerCooling() 
    {
        cooling = true;
    }
}
