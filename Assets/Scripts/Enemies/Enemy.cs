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
    public float jumpForce = 12f; // <-- Ajustado para que alcance a subir bien
    private float currentWalkSpeed;
    private int direction = 1;

    [Header("Inteligencia (Sensor de Abismos)")]
    public Transform detectorSuelo; 
    public float distanciaDeteccion = 0.5f;
    public LayerMask capaSuelo;

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

        ActualizarMirada();
    }

    void Update()
    {
        HandleThawing();

        switch (currentState)
        {
            case State.Walking:
                // 1. Moverse
                rb.linearVelocity = new Vector2(direction * currentWalkSpeed, rb.linearVelocity.y);

                // Verificamos si tocamos piso
                bool tocandoPiso = Physics2D.Raycast(transform.position, Vector2.down, 1.5f, capaSuelo);
                
                if (!tocandoPiso) 
                {
                    Debug.Log("ERROR: No estoy tocando piso. Revisa la capa del suelo.");
                }

                // 2. Abismos
                if (detectorSuelo != null)
                {
                    RaycastHit2D haySuelo = Physics2D.Raycast(detectorSuelo.position, Vector2.down, distanciaDeteccion, capaSuelo);
                    if (haySuelo.collider == false && tocandoPiso)
                    {
                        Girar();
                    }
                }

                // 3. Radar hacia arriba
                if (tocandoPiso)
                {
                    RaycastHit2D plataformaArriba = Physics2D.Raycast(transform.position, Vector2.up, 3.0f, capaSuelo);
                    
                    if (plataformaArriba.collider != null)
                    {
                        if (plataformaArriba.distance > 1f)
                        {
                            Debug.Log("¡Veo una plataforma arriba! Preparando salto...");
                            if (Random.Range(0, 20) < 1) 
                            {
                                Debug.Log("¡BRINCO!");
                                Saltar();
                            }
                        }
                        else
                        {
                            Debug.Log("La plataforma me está aplastando la cabeza (muy cerca).");
                        }
                    }
                }
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

    void Saltar()
    {
        // Le damos un empujón hacia arriba manteniendo su velocidad horizontal
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
    }

    void Girar()
    {
        direction *= -1;
        ActualizarMirada();
    }

    void ActualizarMirada()
    {
        if (sr != null)
        {
            sr.flipX = (direction == -1);
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
        ActualizarMirada(); 
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
            Girar();
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
                    GameManager.Instance.CheckLevelClear();
                }

                Destroy(collision.gameObject);
            }
        }

        if (currentState == State.Walking && (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Enemy")))
        {
            Girar();
        }
    }

    void CheckAndDestroy()
    {
        if (GameManager.Instance != null)
            GameManager.Instance.CheckLevelClear();

        Destroy(gameObject);
    }
}