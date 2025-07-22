using System.Collections;
using UnityEngine;

public class KeyInteraction : MonoBehaviour
{
    [Header("Configuration")]
    public FadingDoor targetDoor; // Reference to the door that this key opens
    public KeyCode interactionKey = KeyCode.E; // The key to press for interaction
    public float delayOpen = 2f;


    [Header("UI Prompt")]
    public GameObject interactionPromptUI; // The UI element to show when interaction is possible

    private bool isPlayerNearby = false;

    /// <summary>
    /// Called when the script instance is being loaded to ensure the UI is initially hidden.
    /// </summary>
    private void Awake()
    {
        if (interactionPromptUI != null)
        {
            interactionPromptUI.SetActive(false);
        }
    }

    /// <summary>
    /// Called every frame to check for player input when they are nearby.
    /// </summary>
    void Update()
    {
        if (isPlayerNearby && Input.GetKeyDown(interactionKey))
        {
            Interact();
        }
    }

    /// <summary>
    /// Executes the core interaction logic, opening the door and consuming the key.
    /// </summary>
    private void Interact()
    {
        Debug.Log("Key collected! Opening the door.");

        if (interactionPromptUI != null)
        {
            interactionPromptUI.SetActive(false);
        }

        if (targetDoor != null && FindObjectOfType<CameraSwitcher>() != null)
        {
            FindObjectOfType<CameraSwitcher>()?.SummonFocusChest();
            StartCoroutine(DelayForOpenDoor());
        }
        else 
        {
            targetDoor.Open();
            // The key is consumed, so destroy this GameObject
            Destroy(gameObject);
        }
      
    }

    /// <summary>
    /// Called when another object enters this object's trigger collider.
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = true;
            if (interactionPromptUI != null)
            {
                interactionPromptUI.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Called when another object exits this object's trigger collider.
    /// </summary>
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerNearby = false;
            if (interactionPromptUI != null)
            {
                interactionPromptUI.SetActive(false);
            }
        }
    }

    private IEnumerator DelayForOpenDoor()
    {
        yield return new WaitForSeconds(delayOpen);
        targetDoor.Open();
        // The key is consumed, so destroy this GameObject
        Destroy(gameObject);
    }
}