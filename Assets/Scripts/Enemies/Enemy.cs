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
    public float baseWalkSpeed = 2f;
    public float rollSpeed = 10f;
    private float currentWalkSpeed;

    [Header("Ball System (Nieve/Balón)")]
    public int hitsToFreeze = 3;
    private int currentHits = 0;

    [Header("Thawing (Descongelamiento)")]
    public float thawTimeWalk = 3f; // Segundos para perder un impacto si sigue caminando
    public float thawTimeBall = 5f; // Segundos para liberarse si ya es un balón
    private float thawTimer = 0f;

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
        currentWalkSpeed = baseWalkSpeed;
    }

    void Update()
    {
        HandleThawing();

        switch (currentState)
        {
            case State.Walking:
                rb.linearVelocity = new Vector2(direction * currentWalkSpeed, rb.linearVelocity.y);
                break;

            case State.Ball:
                rb.linearVelocity = Vector2.zero;
                // Efecto visual retro: El balón "tiembla" cuando está a punto de liberarse
                if (thawTimer > thawTimeBall * 0.7f)
                {
                    transform.position += new Vector3(Mathf.Sin(Time.time * 50f) * 0.02f, 0, 0);
                }
                break;

            case State.Rolling:
                rb.linearVelocity = new Vector2(direction * rollSpeed, rb.linearVelocity.y);
                break;
        }
    }

    // ⏱ Sistema de tiempo para perder impactos
    void HandleThawing()
    {
        // Si no tiene impactos o ya está rodando como ataque, no se descongela
        if (currentHits == 0 || currentState == State.Rolling) return;

        thawTimer += Time.deltaTime;
        float timeLimit = (currentState == State.Ball) ? thawTimeBall : thawTimeWalk;

        if (thawTimer >= timeLimit)
        {
            thawTimer = 0f;
            currentHits--;

            if (currentState == State.Ball)
            {
                currentState = State.Walking;
                // En el arcade original, salen furiosos. Podríamos aumentar su velocidad aquí luego.
            }

            UpdateVisualsAndSpeed();
        }
    }

    // ⚽ Recibe impacto de balonazo
    public void TakeSnowHit()
    {
        if (currentState != State.Walking && currentState != State.Ball) return;

        thawTimer = 0f; // Reseteamos el reloj porque acaba de recibir un golpe

        if (currentHits < hitsToFreeze)
        {
            currentHits++;
            if (currentHits >= hitsToFreeze)
            {
                currentState = State.Ball;
                // Ajustamos la posición para que quede a ras de suelo y no flote al ser balón
                rb.linearVelocity = Vector2.zero;
            }
        }

        UpdateVisualsAndSpeed();
    }

    // 🎨 Actualiza color y velocidad según el daño
    void UpdateVisualsAndSpeed()
    {
        // Ralentización progresiva: nunca llega a cero hasta ser balón (mínimo 20% de velocidad)
        float slowFactor = 1f - ((float)currentHits / hitsToFreeze);
        currentWalkSpeed = baseWalkSpeed * Mathf.Max(slowFactor, 0.2f);

        // Cambio visual progresivo hacia cyan (o el color del balón de la copa del mundo)
        sr.color = Color.Lerp(Color.white, Color.cyan, (float)currentHits / hitsToFreeze);
    }

    // 🦵 Patada (Se mantiene igual a tu lógica original)
    public void Kick(int dir)
    {
        if (currentState != State.Ball) return;

        direction = dir;
        bounceCount = 0;
        comboCount = 0;
        currentState = State.Rolling;

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
        if (currentState == State.Rolling && collision.gameObject.CompareTag("Wall"))
        {
            direction *= -1;
            bounceCount++;

            if (bounceCount >= maxBounces)
            {
                CheckAndDestroy();
            }
        }

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

                if (GameManager.Instance != null)
                    GameManager.Instance.CheckLevelClear();
            }
        }

        if (currentState == State.Walking && collision.gameObject.CompareTag("Wall"))
        {
            direction *= -1;
        }
    }

    void CheckAndDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.CheckLevelClear();

        Destroy(gameObject);
    }
}