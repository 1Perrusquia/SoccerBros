using UnityEngine;

public class Enemy : MonoBehaviour
{
    public enum State
    {
        Walking,
        Ball,
        Rolling
    }

    public State currentState = State.Walking;

    [Header("Movement")]
    public float walkSpeed = 2f;
    public float rollSpeed = 10f;

    [Header("Snow System")]
    public int hitsToFreeze = 3;
    private int currentHits = 0;

    [Header("Rolling")]
    public int maxBounces = 3;
    private int bounceCount = 0;

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
        switch (currentState)
        {
            case State.Walking:
                rb.linearVelocity = new Vector2(direction * walkSpeed, rb.linearVelocity.y);
                break;

            case State.Ball:
                rb.linearVelocity = Vector2.zero;
                break;

            case State.Rolling:
                rb.linearVelocity = new Vector2(direction * rollSpeed, rb.linearVelocity.y);
                break;
        }
    }

    // ❄ Se llama desde Snowball
    public void TakeSnowHit()
    {
        if (currentState != State.Walking)
            return;

        currentHits++;

        // Cambia color gradualmente
        sr.color = Color.Lerp(Color.white, Color.cyan, (float)currentHits / hitsToFreeze);

        if (currentHits >= hitsToFreeze)
        {
            currentState = State.Ball;
        }
    }

    // ⚽ Se llama cuando el jugador dispara cerca
    public void Kick(int dir)
    {
        if (currentState != State.Ball)
            return;

        direction = dir;
        bounceCount = 0;
        currentState = State.Rolling;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Rebote contra pared
        if (currentState == State.Rolling && collision.gameObject.CompareTag("Wall"))
        {
            direction *= -1;
            bounceCount++;

            if (bounceCount >= maxBounces)
            {
                Destroy(gameObject);
            }
        }

        // Mata enemigos
        if (currentState == State.Rolling && collision.gameObject.CompareTag("Enemy"))
        {
            if (collision.gameObject != gameObject)
            {
                Destroy(collision.gameObject);
            }
        }

        // Cambio dirección al caminar
        if (currentState == State.Walking && collision.gameObject.CompareTag("Wall"))
        {
            direction *= -1;
        }
    }
}