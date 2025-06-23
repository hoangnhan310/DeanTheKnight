using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogue;
    private bool dialogueTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !dialogueTriggered)
        {
            FindAnyObjectByType<DialogueManager>().StartDialogue(dialogue);
            dialogueTriggered = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            FindAnyObjectByType<DialogueManager>().EndDialogue();
            dialogueTriggered = false;
        }
    }
}