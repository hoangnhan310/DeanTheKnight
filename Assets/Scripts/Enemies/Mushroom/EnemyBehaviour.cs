using System.Collections;
using System.Linq;
using UnityEditor.Tilemaps;
using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    #region Public Variables
    public float attackDistance; //Minimum distance to attack the player
    public float moveSpeed;
    public float timer; //Time for cooldown between attacks
    public Transform leftLimit; //Left limit for the enemy to move
    public Transform rightLimit; //Right limit for the enemy to move
    [HideInInspector] public Transform target;
    [HideInInspector] public bool inRange; //Check if the player is in range
    public GameObject hotZone;
    public GameObject triggerArea;
    public int maxHealth = 100; //Maximum health of the enemy
    public bool IsDead => isDead; //Property to check if the enemy is dead
    #endregion

    #region Private Variables
    private Animator animator;
    private float distanceToTarget; //Store the distance b/w enemy and player
    private bool attackMode;
    private bool cooling; //Check if the enemy is cooling down after an attack
    private float intTimer;
    private float currentHealth; //Current health of the enemy
    private bool isDead; //Check if the enemy is dead
    #endregion

    private void Awake()
    {

        SelectTarget(); //Select a target when the enemy is spawned
        intTimer = timer; //Store the initial timer value
        animator = GetComponent<Animator>();
        currentHealth = maxHealth; //Initialize current health to max health
    }

    void Update()
    {
        if (isDead) return; //If the enemy is dead, do not execute the rest of the code
        if (!attackMode)
        {
            Move();
        }

        if (!insideOfLimits() && !inRange  && !animator.GetCurrentAnimatorStateInfo(0).IsName("Attack")) 
        { 
            SelectTarget(); //Select a target when the enemy is outside of limits
        }


        if (inRange) 
        {
            EnemyLogic();
        }
    }

    public void EnemyLogic() 
    { 
        distanceToTarget = Vector2.Distance(transform.position, target.position);
        if (distanceToTarget > attackDistance) 
        {
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
            Vector2 targetPosition = new Vector2(target.position.x, transform.position.y);

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

    public void TriggerCooling() 
    {
        cooling = true;
    }

    private bool insideOfLimits() 
    { 
        return transform.position.x > leftLimit.position.x && transform.position.x < rightLimit.position.x;
    }

    public void SelectTarget() 
    {
        float distanceToLeft = Vector2.Distance(transform.position, leftLimit.position);
        float distanceToRight = Vector2.Distance(transform.position, rightLimit.position);

        if (distanceToLeft > distanceToRight) 
        {
            target = leftLimit;
        }
        else
        {
            target = rightLimit;
        }

        Flip();
    }

    public void Flip() 
    {
        Vector3 rotation = transform.eulerAngles;
        //Flip the enemy if out of limits
        if (transform.position.x > target.position.x)
        {
            rotation.y = 0f;
        }
        else 
        { 
            rotation.y = 180f;
        }

        transform.eulerAngles = rotation;
    }

    public void TakeDamage(float damage) 
    {
        currentHealth -= damage; //Reduce the current health by damage
        animator.SetTrigger("Hit"); //Set the hit animation
        if (currentHealth <= 0) 
        {
            Die(); //Call the Die method if health is 0 or less
        }
    }

    public void Die() 
    {   Debug.Log("Enemy died"); //Log the death of the enemy
        isDead = true; //Set the enemy as dead
        animator.SetBool("IsDead", isDead); //Set the die animation
        StartCoroutine(DeathDelay());
    }

    private IEnumerator DeathDelay()
    {
        yield return new WaitForSeconds(1.5f); // Time for animation die
        this.hotZone.SetActive(false); //Disable the hot zone
        this.triggerArea.SetActive(false); //Disable the trigger area
        animator.enabled = false; //Disable the animator     
        GetComponent<Collider2D>().enabled = false;
        GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static; //Set the rigidbody to static
        GetComponentsInChildren<CapsuleCollider2D>().ToList().ForEach(c => c.enabled = false); //Disable all capsule colliders
        this.enabled = false;
    }
}
