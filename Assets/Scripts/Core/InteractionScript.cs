using UnityEngine;

public class InteractionScript : MonoBehaviour
{
    public string interactionKey = "e";      // Phím bấm
    public string triggerName = "Activate";  // Tên trigger trong Animator

    private bool playerInRange = false;
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactionKey))
        {
            animator.SetTrigger(triggerName);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
