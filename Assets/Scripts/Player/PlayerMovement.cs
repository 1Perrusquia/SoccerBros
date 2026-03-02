using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    public GameObject snowballPrefab;
    public Transform firePoint;

    private bool facingRight = true;
    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (isDead) return;

        float moveX = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);

        if (moveX > 0) facingRight = true;
        if (moveX < 0) facingRight = false;

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        float kickRadius = 1f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, kickRadius);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Enemy enemy = hit.GetComponent<Enemy>();

                if (enemy != null && enemy.currentState == Enemy.State.Ball)
                {
                    int dir = facingRight ? 1 : -1;
                    enemy.Kick(dir);
                    return;
                }
            }
        }

        GameObject snowball = Instantiate(snowballPrefab, firePoint.position, Quaternion.identity);
        Vector2 dirShoot = facingRight ? Vector2.right : Vector2.left;
        snowball.GetComponent<Snowball>().SetDirection(dirShoot);
    }

    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        gameObject.SetActive(false);

        if (GameManager.Instance != null)
            GameManager.Instance.PlayerDied();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;

        if (collision.gameObject.CompareTag("Enemy"))
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();

            // 🔥 SOLO WALKING MATA
            if (enemy != null && enemy.currentState == Enemy.State.Walking)
            {
                Die();
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = false;
    }
}