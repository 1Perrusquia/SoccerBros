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
    private int direction = 1;

    private Rigidbody2D rb;
    private SpriteRenderer sr;

    public enum EnemyState
    {
        Normal,
        FrozenPartial,
        FrozenBall,
        Dead
    }

    public EnemyState currentState = EnemyState.Normal;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        switch (currentState)
        {
            case EnemyState.Normal:
                HandleMovement(1f);
                break;

            case EnemyState.FrozenPartial:
                HandleMovement(0.5f); // Se mueve más lento
                break;

            case EnemyState.FrozenBall:
                rb.linearVelocity = Vector2.zero;
                break;

            case EnemyState.Dead:
                break;
        }

        Flip();
    }

    void HandleMovement(float speedModifier)
    {
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

        rb.linearVelocity = new Vector2(movementX * moveSpeed * speedModifier, rb.linearVelocity.y);
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
        if (currentState == EnemyState.FrozenBall || currentState == EnemyState.Dead)
            return;

        currentHits++;

        if (currentHits >= hitsToFreeze)
        {
            EnterFrozenBall();
        }
        else
        {
            EnterFrozenPartial();
        }
    }

    void EnterFrozenPartial()
    {
        currentState = EnemyState.FrozenPartial;
        sr.color = new Color(0.6f, 0.9f, 1f); // tono azulado leve
    }

    void EnterFrozenBall()
    {
        currentState = EnemyState.FrozenBall;

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 1;
        rb.freezeRotation = true;

        sr.color = Color.cyan;

        PhysicsMaterial2D slippery = new PhysicsMaterial2D();
        slippery.friction = 0f;
        slippery.bounciness = 0f;

        GetComponent<Collider2D>().sharedMaterial = slippery;
    }

    public void Die()
    {
        currentState = EnemyState.Dead;
        Destroy(gameObject);
    }
}