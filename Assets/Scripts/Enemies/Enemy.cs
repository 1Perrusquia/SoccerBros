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
    private int comboCount = 0;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Collider2D myCollider;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        myCollider = GetComponent<Collider2D>();
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

    // ❄ Recibe impacto de nieve
    public void TakeSnowHit()
    {
        if (currentState != State.Walking)
            return;

        currentHits++;

        // Cambio visual progresivo
        sr.color = Color.Lerp(Color.white, Color.cyan, (float)currentHits / hitsToFreeze);

        if (currentHits >= hitsToFreeze)
        {
            currentState = State.Ball;
            rb.linearVelocity = Vector2.zero;
        }
    }

    // ⚽ Patada
    public void Kick(int dir)
    {
        if (currentState != State.Ball)
            return;

        direction = dir;
        bounceCount = 0;
        comboCount = 0;
        currentState = State.Rolling;

        // Ignorar colisión con jugador actual
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            Collider2D playerCollider = player.GetComponent<Collider2D>();
            if (playerCollider != null)
            {
                Physics2D.IgnoreCollision(myCollider, playerCollider, true);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // 🔁 Rebote contra pared
        if (currentState == State.Rolling && collision.gameObject.CompareTag("Wall"))
        {
            direction *= -1;
            bounceCount++;

            if (bounceCount >= maxBounces)
            {
                CheckAndDestroy();
            }
        }

        // 💥 Combo contra otro enemigo
        if (currentState == State.Rolling && collision.gameObject.CompareTag("Enemy"))
        {
            if (collision.gameObject != gameObject)
            {
                comboCount++;

                int points = 500 * (int)Mathf.Pow(2, comboCount - 1);

                if (GameManager.Instance != null)
                {
                    GameManager.Instance.AddScore(points);
                    GameManager.Instance.ShowFloatingText("+" + points, collision.transform.position);
                }

                Destroy(collision.gameObject);

                // Revisar si ya no quedan enemigos
                if (GameManager.Instance != null)
                    GameManager.Instance.CheckLevelClear();
            }
        }

        // 🚶 Cambio dirección caminando
        if (currentState == State.Walking && collision.gameObject.CompareTag("Wall"))
        {
            direction *= -1;
        }
    }

    // ✅ Método seguro para destruir enemigo rodando
    void CheckAndDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.CheckLevelClear();

        Destroy(gameObject);
    }
}