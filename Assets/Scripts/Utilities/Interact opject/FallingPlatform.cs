using System.Collections;
using UnityEngine;

public class FallingPlatform : MonoBehaviour
{
    private Rigidbody2D rb;
    private Vector3 startPos;
    private Quaternion startRot;

    [SerializeField] private float fallDelay = 1f;
    [SerializeField] private float respawnDelay = 3f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        startPos = transform.position;
        startRot = transform.rotation;
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (col.gameObject.CompareTag("Player"))
        {
            StartCoroutine(FallAndRespawn());
        }
    }

    private IEnumerator FallAndRespawn()
    {
        yield return new WaitForSeconds(fallDelay);
        rb.bodyType = RigidbodyType2D.Dynamic;

        yield return new WaitForSeconds(respawnDelay);
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.linearVelocity = Vector2.zero;

        transform.position = startPos;
        transform.rotation = startRot;
    }

}
