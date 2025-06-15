using UnityEngine;

public class BackgroundController : MonoBehaviour
{
    private float startPositionX;
    [SerializeField]
    private GameObject came;
    [SerializeField]
    private float parallaxSpeed = 0.5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        startPositionX = transform.position.x;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float distance = came.transform.position.x * parallaxSpeed; //0 = move with camera || 1 = won't move || 0.5 = half

        transform.position = new Vector3(startPositionX + distance, transform.position.y, transform.position.z);
    }
}
