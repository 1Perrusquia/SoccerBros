using UnityEngine;

public class Boss : MonoBehaviour
{
    [Header("Estadisticas")]
    public int maxHealth = 5;
    private int currentHealth;

    [Header("Invocacion")]
    public GameObject minionPrefab;
    public Transform[] spawnPoints;
    public float spawnRate = 8f;
    public float launchForce = 45f;
    private float spawnTimer;

    [Header("Estado")]
    public bool isPlayerPresent = false;
    public GameObject copaPrefab;

    private Animator anim;
    private SpriteRenderer sr;
    private Collider2D bossCollider;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        bossCollider = GetComponent<Collider2D>();
        spawnTimer = spawnRate / 2f;

        // Aseguramos que el jefe esté en Z = 0 para evitar fallos de colisión
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
    }

    void Update()
    {
        if (!isPlayerPresent || currentHealth <= 0) return;

        // --- PLAN C: DETECCIÓN POR DISTANCIA (POR SI LAS FÍSICAS FALLAN) ---
        GameObject[] enemigos = GameObject.FindGameObjectsWithTag("Enemy");
        foreach (GameObject e in enemigos)
        {
            // Calculamos la distancia entre el jefe y cada enemigo en escena
            float distancia = Vector2.Distance(transform.position, e.transform.position);

            // 1.5f es un buen radio para el tamaño de tu jefe
            if (distancia < 1.5f)
            {
                Enemy enemyScript = e.GetComponent<Enemy>();
                if (enemyScript != null)
                {
                    // Si el enemigo está en estado de balón o rodando
                    if (enemyScript.currentState == Enemy.State.Rolling || enemyScript.currentState == Enemy.State.Ball)
                    {
                        UnityEngine.Debug.Log("¡Impacto detectado por Distancia!");
                        ProcesarImpacto(e);
                    }
                }
            }
        }

        // Lógica de disparo de esbirros
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnRate)
        {
            SpawnMinion();
            spawnTimer = 0f;
        }
    }

    void SpawnMinion()
    {
        if (minionPrefab == null || spawnPoints.Length == 0) return;

        if (anim != null) anim.SetTrigger("Throw");

        int randomIndex = UnityEngine.Random.Range(0, spawnPoints.Length);
        GameObject newMinion = Instantiate(minionPrefab, spawnPoints[randomIndex].position, Quaternion.identity);

        // Ignorar colisión inicial para que no se atore en el jefe
        Collider2D minionCol = newMinion.GetComponent<Collider2D>();
        if (minionCol != null && bossCollider != null)
        {
            Physics2D.IgnoreCollision(minionCol, bossCollider);
        }

        Rigidbody2D rb = newMinion.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.AddForce(Vector2.right * launchForce, ForceMode2D.Impulse);
        }
    }

    public void ActivateBoss()
    {
        isPlayerPresent = true;
        UnityEngine.Debug.Log("Jefe Activado");
    }

    // Detección física normal (por si acaso)
    void OnCollisionEnter2D(Collision2D collision) { ProcesarImpacto(collision.gameObject); }
    void OnTriggerEnter2D(Collider2D other) { ProcesarImpacto(other.gameObject); }

    void ProcesarImpacto(GameObject objeto)
    {
        if (objeto.CompareTag("Enemy"))
        {
            Enemy enemyScript = objeto.GetComponent<Enemy>();
            if (enemyScript != null)
            {
                if (enemyScript.currentState == Enemy.State.Rolling || enemyScript.currentState == Enemy.State.Ball)
                {
                    TakeDamage();
                    enemyScript.Die(true); // El balón explota
                }
            }
        }
    }

    void TakeDamage()
    {
        currentHealth--;
        UnityEngine.Debug.Log("Vida del Jefe: " + currentHealth);

        if (AudioManager.instance != null)
            AudioManager.instance.PlaySFX(AudioManager.instance.sonidoDanoJefe);

        if (anim != null) anim.SetTrigger("Hit");

        sr.color = Color.red;
        Invoke("ResetColor", 0.15f);

        if (currentHealth <= 0) Die();
    }

    void ResetColor()
    {
        if (currentHealth > 0 && sr != null) sr.color = Color.white;
    }

    void Die()
    {
        UnityEngine.Debug.Log("¡Jefe Derrotado!");
        if (AudioManager.instance != null)
            AudioManager.instance.PlaySFX(AudioManager.instance.sonidoVictoria);

        if (anim != null) anim.SetTrigger("Die");
        if (bossCollider != null) bossCollider.enabled = false;

        if (copaPrefab != null)
        {
            Instantiate(copaPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);
        }

        Destroy(gameObject, 2f);
    }
}