using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public enum State { Walking, Ball, Rolling, Dead } // Añadimos estado Dead
    public State currentState = State.Walking;

    [Header("Transformación a Balón")]
    public Sprite spriteBalonFutbol; // Arrastra tu imagen del balón aquí en el Inspector
    private Sprite spriteOriginal;     // El script recordará la imagen del monstruo aquí

    [Header("Movement")]
    public float baseWalkSpeed = 2f;
    public float rollSpeed = 10f;
    public float jumpForce = 12f;
    private float currentWalkSpeed;
    private int direction = 1;

    [Header("Inteligencia (Sensores)")]
    public Transform detectorSuelo;
    public float distanciaDeteccionSuelo = 0.5f;
    public float distanciaRayoPiso = 1.6f;
    public LayerMask capaSuelo;

    [Header("Ball System")]
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

    [Header("Premios")]
    public GameObject[] powerUpPrefabs;

    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private Collider2D myCollider;
    private Animator anim; // El cerebro de las animaciones

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        myCollider = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();

        // Guardamos cómo se ve normalmente para cuando se descongele
        spriteOriginal = sr.sprite;
        currentWalkSpeed = baseWalkSpeed;

        ActualizarMirada();
    }

    void Update()
    {
        HandleThawing();

        switch (currentState)
        {
            case State.Walking:
                rb.linearVelocity = new Vector2(direction * currentWalkSpeed, rb.linearVelocity.y);

                UnityEngine.Debug.DrawRay(transform.position, Vector2.down * distanciaRayoPiso, Color.red);
                bool tocandoPiso = Physics2D.Raycast(transform.position, Vector2.down, distanciaRayoPiso, capaSuelo);

                if (detectorSuelo != null)
                {
                    RaycastHit2D haySueloEnBorde = Physics2D.Raycast(detectorSuelo.position, Vector2.down, distanciaDeteccionSuelo, capaSuelo);
                    if (haySueloEnBorde.collider == null && tocandoPiso)
                    {
                        Girar();
                    }
                }

                if (tocandoPiso)
                {
                    RaycastHit2D plataformaArriba = Physics2D.Raycast(transform.position, Vector2.up, 3.0f, capaSuelo);
                    if (plataformaArriba.collider != null && plataformaArriba.distance > 1.2f)
                    {
                        if (UnityEngine.Random.Range(0, 500) < 2)
                        {
                            Saltar();
                        }
                    }
                }
                break;

            case State.Ball:
                rb.linearVelocity = Vector2.zero;
                // Vibrar antes de descongelarse
                if (thawTimer > thawTimeBall * 0.7f)
                {
                    transform.position += new Vector3(Mathf.Sin(Time.time * 50f) * 0.02f, 0, 0);
                }
                break;

            case State.Rolling:
                rb.linearVelocity = new Vector2(direction * rollSpeed, rb.linearVelocity.y);
                // Si quieres que el balón gire mientras rueda, puedes descomentar la siguiente línea:
                // transform.Rotate(0, 0, -direction * rollSpeed * 2f);
                break;

            case State.Dead:
                rb.linearVelocity = Vector2.zero;
                break;
        }
    }

    void Saltar()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        if (anim != null) anim.SetTrigger("Jump"); // Dispara la animación de salto
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
            // Como tus sprites originales miran hacia la IZQUIERDA:
            // Si se mueven a la derecha (direction == 1), hay que voltearlos (true).
            sr.flipX = (direction == 1);
        }
    }

    void HandleThawing()
    {
        if (currentHits == 0 || currentState == State.Rolling || currentState == State.Dead) return;

        thawTimer += Time.deltaTime;
        float timeLimit = (currentState == State.Ball) ? thawTimeBall : thawTimeWalk;

        if (thawTimer >= timeLimit)
        {
            thawTimer = 0f;
            currentHits--;

            if (currentState == State.Ball && currentHits == 0)
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
        // Magia de Transformación
        if (currentHits >= hitsToFreeze)
        {
            // Apagamos la animación y cambiamos la imagen por el balón
            if (anim != null) anim.enabled = false;
            sr.sprite = spriteBalonFutbol;
            sr.color = Color.white; // Reseteamos el color por si estaba azulado

            // Opcional: Enderezamos la rotación por si rodó antes
            transform.rotation = Quaternion.identity;
        }
        else
        {
            // Lo regresamos a la normalidad
            if (anim != null) anim.enabled = true;
            sr.sprite = spriteOriginal;

            float slowFactor = 1f - ((float)currentHits / hitsToFreeze);
            currentWalkSpeed = baseWalkSpeed * Mathf.Max(slowFactor, 0.2f);
            sr.color = Color.Lerp(Color.white, Color.cyan, (float)currentHits / hitsToFreeze);
        }
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
            Physics2D.IgnoreCollision(myCollider, player.GetComponent<Collider2D>(), true);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState == State.Rolling && (collision.gameObject.CompareTag("Wall") || ((1 << collision.gameObject.layer) & capaSuelo) != 0))
        {
            Girar();
            bounceCount++;

            if (bounceCount >= maxBounces)
            {
                Die(false); // Destrucción silenciosa (se deshace la bola)
            }
        }

        // Si este enemigo va rodando y arrolla a otro enemigo
        if (currentState == State.Rolling && collision.gameObject.CompareTag("Enemy"))
        {
            if (collision.gameObject != gameObject)
            {
                comboCount++;

                // Le decimos al OTRO enemigo que fue arrollado que ejecute su animación de muerte
                Enemy otherEnemy = collision.gameObject.GetComponent<Enemy>();
                if (otherEnemy != null)
                {
                    otherEnemy.Die(true); // El otro muere con animación
                }
            }
        }

        if (currentState == State.Walking && (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Enemy")))
        {
            Girar();
        }
    }

    // Nueva función para manejar la muerte visualmente
    public void Die(bool playAnimation)
    {
        if (currentState == State.Dead) return;

        currentState = State.Dead;
        myCollider.enabled = false; // Apagamos su colisión para que no moleste
        rb.gravityScale = 0; // Evitamos que caiga al vacío si muere en el aire
        rb.linearVelocity = Vector2.zero;

        GenerarPremio(transform.position);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.Invoke("CheckLevelClear", 0.1f);
        }

        if (playAnimation && anim != null)
        {
            anim.enabled = true; // Por si estaba convertido en balón
            anim.SetTrigger("Die");
            Destroy(gameObject, 0.6f); // Esperamos un poco a que termine la animación
        }
        else
        {
            Destroy(gameObject); // Se destruye al instante
        }
    }

    void GenerarPremio(Vector3 posicion)
    {
        if (powerUpPrefabs != null && powerUpPrefabs.Length > 0)
        {
            int indiceElegido = UnityEngine.Random.Range(0, powerUpPrefabs.Length);
            Instantiate(powerUpPrefabs[indiceElegido], posicion, Quaternion.identity);
        }
    }
}