using System;
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
    public float thawTimeWalk = 3f;
    public float thawTimeBall = 5f;
    private float thawTimer = 0f;

    [Header("Rolling")]
    public int maxBounces = 3;
    private int bounceCount = 0;
    private int direction = 1;
    private int comboCount = 0;

    [Header("Premios y Power Ups")]
    public GameObject[] powerUpPrefabs;

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

    void HandleThawing()
    {
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
            }

            UpdateVisualsAndSpeed();
        }
    }

    public void TakeSnowHit()
    {
        if (currentState != State.Walking && currentState != State.Ball) return;

        thawTimer = 0f;

        if (currentHits < hitsToFreeze)
        {
            currentHits++;
            if (currentHits >= hitsToFreeze)
            {
                currentState = State.Ball;
                rb.linearVelocity = Vector2.zero;
            }
        }

        UpdateVisualsAndSpeed();
    }

    void UpdateVisualsAndSpeed()
    {
        float slowFactor = 1f - ((float)currentHits / hitsToFreeze);
        currentWalkSpeed = baseWalkSpeed * Mathf.Max(slowFactor, 0.2f);
        sr.color = Color.Lerp(Color.white, Color.cyan, (float)currentHits / hitsToFreeze);
    }

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

                // =========================================================
                // <--- AGREGADO: ¡Soltamos un premio justo donde muere este enemigo!
                // =========================================================
                GenerarPremio(collision.transform.position);

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
        // =========================================================
        // <--- AGREGADO: La bola gigante también suelta su propio premio al explotar
        // =========================================================
        GenerarPremio(transform.position);

        if (GameManager.Instance != null)
            GameManager.Instance.CheckLevelClear();

        Destroy(gameObject);
    }

    // =====================================================================
    // <--- NUEVA FUNCIÓN: Nuestra "Fábrica de Premios" que podemos usar en cualquier lugar
    // =====================================================================
    void GenerarPremio(Vector3 posicionDeAparicion)
    {
        if (powerUpPrefabs != null && powerUpPrefabs.Length > 0)
        {
            int indiceElegido = 0;

            if (powerUpPrefabs.Length > 3)
            {
                float suerte = UnityEngine.Random.value;

                if (suerte <= 0.20f)
                {
                    // 20% de probabilidad: Soltar una Poción (Índices 0, 1 o 2)
                    indiceElegido = UnityEngine.Random.Range(0, 3);
                }
                else
                {
                    // 80% de probabilidad: Soltar Comida Mexicana (Del índice 3 en adelante)
                    indiceElegido = UnityEngine.Random.Range(3, powerUpPrefabs.Length);
                }
            }
            else
            {
                indiceElegido = UnityEngine.Random.Range(0, powerUpPrefabs.Length);
            }

            // Aparece el premio en la posición exacta que le mandemos
            Instantiate(powerUpPrefabs[indiceElegido], posicionDeAparicion, Quaternion.identity);
        }
    }
}