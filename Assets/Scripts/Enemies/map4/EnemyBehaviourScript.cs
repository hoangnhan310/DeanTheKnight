using System.Collections;
using UnityEngine;

public class EnemyBehaviourScript : MonoBehaviour
{
    [Header("Health Settings")]
    [SerializeField] private float maxHealth = 100f; // Máu tối đa của kẻ thù
    private float currentHealth; // Máu hiện tại

    [Header("Animation Timings")]
    [SerializeField] private float hitAnimationDuration = 0.5f; // Thời gian chạy animation bị đánh
    [SerializeField] private float dieAnimationDuration = 1.5f; // Thời gian chạy animation chết trước khi vô hiệu hóa GameObject

    // References
    private Animator animController; // Tham chiếu tới Animator component
    private Collider2D mainCollider; // Tham chiếu tới Collider2D chính của kẻ thù
    private Rigidbody2D rb2d; // Tham chiếu tới Rigidbody2D

    private bool isDead = false; // Cờ kiểm tra trạng thái sống/chết
    private bool isHurt = false; // Cờ kiểm tra trạng thái bị đánh

    // Public property để script khác có thể kiểm tra trạng thái chết
    public bool IsDead => isDead;

    private void Awake()
    {
        animController = GetComponent<Animator>();
        mainCollider = GetComponent<Collider2D>();
        rb2d = GetComponent<Rigidbody2D>();

        if (animController == null)
        {
            Debug.LogError("Animator not found on " + gameObject.name + ". Please add an Animator component.");
        }
        if (mainCollider == null)
        {
            Debug.LogWarning("Collider2D not found on " + gameObject.name + ". Damage detection might not work.");
        }

        currentHealth = maxHealth;
    }

    private void Update()
    {
        // Nếu kẻ thù đã chết hoặc đang bị đánh, không chạy logic di chuyển/tấn công bình thường
        if (isDead || isHurt)
        {
            return;
        }

        // (Ở đây bạn sẽ có logic di chuyển, tấn công... khi không bị đánh và không chết)
        // Ví dụ: Luôn chơi animation "Move" nếu không bị đánh và không chết
        if (animController != null && !animController.GetCurrentAnimatorStateInfo(0).IsName("Move"))
        {
            // Sử dụng CrossFade để chuyển đổi mượt mà từ trạng thái hiện tại sang "Move"
            // Tùy chỉnh 0.2f là thời gian blending
            animController.CrossFade("Move", 0.2f);
        }
    }

    /// <summary>
    /// Hàm này được gọi khi kẻ thù nhận sát thương.
    /// </summary>
    /// <param name="damage">Lượng sát thương nhận vào.</param>
    public void TakeDamage(float damage)
    {
        if (isDead || isHurt) // Không nhận sát thương nếu đã chết hoặc đang trong trạng thái bị đánh
        {
            return;
        }

        currentHealth -= damage; // Giảm máu
        Debug.Log($"{gameObject.name} took {damage} damage. Current Health: {currentHealth}");

        // Kích hoạt animation bị đánh trực tiếp
        if (animController != null)
        {
            // Sử dụng Play để chuyển ngay lập tức sang animation "hit"
            animController.Play("hit"); // Tên trạng thái/clip animation phải khớp chính xác
        }

        // Đặt cờ bị đánh và bắt đầu coroutine để xử lý thời gian bị đánh
        isHurt = true;
        StartCoroutine(HandleHitSequence());

        // Kiểm tra nếu máu về 0 hoặc ít hơn
        if (currentHealth <= 0)
        {
            Die(); // Gọi hàm chết
        }
    }

    /// <summary>
    /// Coroutine xử lý chuỗi hành động khi bị đánh (chờ animation, sau đó quay lại trạng thái bình thường).
    /// </summary>
    private IEnumerator HandleHitSequence()
    {
        yield return new WaitForSeconds(hitAnimationDuration); // Chờ hết animation hit

        isHurt = false; // Đặt lại cờ bị đánh

        // Sau khi hit xong, nếu chưa chết, quay lại trạng thái Move
        if (!isDead && animController != null)
        {
            // Có thể quay lại "Move" hoặc "Idle" tùy vào logic của bạn
            animController.CrossFade("Move", 0.2f); // Chuyển mượt về Move
        }
    }


    /// <summary>
    /// Hàm này xử lý các hành vi khi kẻ thù chết.
    /// </summary>
    private void Die()
    {
        if (isDead) // Đảm bảo hàm Die chỉ được gọi một lần
        {
            return;
        }

        isDead = true; // Đặt cờ đã chết
        isHurt = false; // Đảm bảo không còn ở trạng thái bị đánh nữa
        Debug.Log($"{gameObject.name} has died!");

        // Kích hoạt animation chết trực tiếp
        if (animController != null)
        {
            // Sử dụng Play để chuyển ngay lập tức sang animation "die"
            animController.Play("die"); // Tên trạng thái/clip animation phải khớp chính xác
        }

        // Bắt đầu Coroutine để vô hiệu hóa GameObject sau khi animation chết hoàn tất
        StartCoroutine(HandleDeathSequence());
    }

    /// <summary>
    /// Coroutine xử lý chuỗi hành động sau khi kẻ thù chết (chờ animation, vô hiệu hóa components).
    /// </summary>
    private IEnumerator HandleDeathSequence()
    {
        // Chờ thời gian bằng thời lượng animation chết
        yield return new WaitForSeconds(dieAnimationDuration);

        // Vô hiệu hóa Collider để không còn va chạm
        if (mainCollider != null)
        {
            mainCollider.enabled = false;
        }
        // Đặt Rigidbody về Static để không còn bị ảnh hưởng bởi vật lý (nếu có)
        if (rb2d != null)
        {
            rb2d.bodyType = RigidbodyType2D.Static;
        }

        // Tắt script này để ngăn chặn logic chạy tiếp
        this.enabled = false;

        // Tùy chọn: Hủy hoặc ẩn GameObject
        // Destroy(gameObject); // Hủy hoàn toàn GameObject
        gameObject.SetActive(false); // Ẩn GameObject
    }
}
