using UnityEngine;

public class Damageable : MonoBehaviour
{
    [SerializeField]
    private int _maxHealth = 100; // Số máu tối đa
    [SerializeField]
    private int _health = 100; // Số máu hiện tại
    [SerializeField]
    private bool _isAlive = true;

    [SerializeField]
    private float invincibilityTime = 1.0f; // Thời gian bất tử (giây)
    private bool isInvincible = false;
    private float timeSinceHit = 0f;

    private Animator animator;

    public int MaxHealth
    {
        get { return _maxHealth; }
        set { _maxHealth = value; }
    }

    public int Health
    {
        get { return _health; }
        set
        {
            _health = value;
            // Nếu health drops below 0, character is no longer alive
            if (_health <= 0)
            {
                _health = 0; // Đảm bảo health không âm
                _isAlive = false;
            }
        }
    }

    public bool IsAlive
    {
        get { return _isAlive; }
        set
        {
            _isAlive = value;
            animator.SetBool("isAlive", value);
            Debug.Log("Character is alive: " + value);  
        }
    }

    private void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogWarning("Animator not found on " + gameObject.name);
        }
    }

    private void Update()
    {
        if (isInvincible)
        {
            if (timeSinceHit > invincibilityTime)
            {
                // Remove invincibility
                isInvincible = false;
                timeSinceHit = 0;
            }
            timeSinceHit += Time.deltaTime;

        }
        Hit(10);
    }

    public void Hit(int damage)
    {
        if (IsAlive && !isInvincible)
        {
            Health -= damage;
            isInvincible = true;
        }
    }
}
