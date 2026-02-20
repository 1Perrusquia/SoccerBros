using UnityEngine;

public class Enemy : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 2f;
    public Transform player;
    public float attractionStrength = 0.4f;
    public float detectionRange = 5f;

    [Header("Freeze Settings")]
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

        float movementX = direction;

        if (player != null)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            if (distanceToPlayer < detectionRange)
            {
                float directionToPlayer = Mathf.Sign(player.position.x - transform.position.x);
                movementX += directionToPlayer * attractionStrength;
            }
        }

        rb.linearVelocity = new Vector2(movementX * moveSpeed, rb.linearVelocity.y);

        Flip();
    }

    void Flip()
    {
        if (rb.linearVelocity.x > 0)
            sr.flipX = false;
        else if (rb.linearVelocity.x < 0)
            sr.flipX = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Wall"))
        {
            direction *= -1;
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
        rb.gravityScale = 1; // vuelve a tener gravedad
        rb.freezeRotation = true;

        sr.color = Color.cyan;

        // Reduce fricción para que se deslice
        PhysicsMaterial2D slippery = new PhysicsMaterial2D();
        slippery.friction = 0f;
        slippery.bounciness = 0f;

        GetComponent<Collider2D>().sharedMaterial = slippery;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (!isFrozen) return;

        if (collision.gameObject.CompareTag("Enemy"))
        {
            float impactForce = rb.linearVelocity.magnitude;

            if (impactForce > 3f)
            {
                GameManager.Instance.AddScore(100);
                Destroy(collision.gameObject);
            }
        }
    }

}
