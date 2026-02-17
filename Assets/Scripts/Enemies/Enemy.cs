using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float moveSpeed = 2f;
    public int hitsToFreeze = 3;

    private int currentHits = 0;
    private bool isFrozen = false;
    private int direction = 1;

    private Rigidbody2D rb;
    private SpriteRenderer sr;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (isFrozen) return;

        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            direction *= -1;
            sr.flipX = !sr.flipX;
        }
    }

    public void TakeHit()
    {
        if (isFrozen) return;

        currentHits++;

        if (currentHits >= hitsToFreeze)
        {
            FreezeEnemy();
        }
    }

    void FreezeEnemy()
    {
        isFrozen = true;
        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0;
        sr.color = Color.white;
    }
}
