using System.Collections;
using UnityEngine;

public class InteractionScript : MonoBehaviour
{
    public string interactionKey = "e";
    public string triggerName = "Activate";
    public GameObject stageClearedCanvas;

    private bool playerInRange = false;
    private Animator animator;
    private bool hasOpened = false;

    void Start()
    {
        animator = GetComponent<Animator>();
        if (stageClearedCanvas != null)
            stageClearedCanvas.SetActive(false);
    }

    void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactionKey) && !hasOpened)
        {
            animator.SetTrigger(triggerName);
            hasOpened = true;

            if (stageClearedCanvas != null)
            {
                stageClearedCanvas.SetActive(true);
            }
            StartCoroutine(ShowStageClearedAfterDelay(0.1f));
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
    private IEnumerator ShowStageClearedAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (stageClearedCanvas != null)
        {
            stageClearedCanvas.SetActive(true);
        }

        Time.timeScale = 0f;
    }

}
