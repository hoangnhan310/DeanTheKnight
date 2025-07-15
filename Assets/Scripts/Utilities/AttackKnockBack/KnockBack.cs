using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

public class KnockBack : MonoBehaviour
{
    [SerializeField]    
    private Rigidbody2D rb; // Reference to the Rigidbody2D component
    [SerializeField]
    private float strength = 5f, delay = 0.15f; // The force and delay applied during knockback

    public UnityEvent OnBegin, OnEnd; // Unity events to trigger actions at the start and end of knockback

    public void PlayFeedBack(GameObject sender) 
    { 
        StopAllCoroutines(); // Stop any ongoing knockback coroutines
        OnBegin?.Invoke(); // Invoke the OnBegin event
        Vector2 direction = (transform.position - sender.transform.position).normalized; // Calculate the direction of knockback
        rb.AddForce(direction*strength, ForceMode2D.Impulse);
        StartCoroutine(Reset()); // Start the coroutine to reset the Rigidbody2D after a delay
    }

    private IEnumerator Reset() 
    { 
        yield return new WaitForSeconds(delay); // Wait for the specified delay
        rb.linearVelocity = Vector3.zero; // Reset the Rigidbody2D's velocity to zero
        OnEnd.Invoke(); // Invoke the OnEnd event
    }
}

