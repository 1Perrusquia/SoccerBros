using UnityEngine;

public class Boss : MonoBehaviour
{
    [Header("Estadisticas")]
    public int maxHealth = 5;
    private int currentHealth;

    [Header("Invocacion")]
    public GameObject minionPrefab;
    public Transform[] spawnPoints;
    public float spawnRate = 10f;
    public float launchForce = 30f;
    private float spawnTimer;

    [Header("Estado")]
    public bool isPlayerPresent = false;
    public GameObject copaPrefab;

    private Animator anim;
    private SpriteRenderer sr;

    void Start()
    {
        currentHealth = maxHealth;
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        spawnTimer = spawnRate / 2f;
    }

    void Update()
    {
        if (!isPlayerPresent || currentHealth <= 0) return;

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

        Rigidbody2D rb = newMinion.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.AddForce(Vector2.left * launchForce, ForceMode2D.Impulse);
        }
    }

    public void ActivateBoss()
    {
        isPlayerPresent = true;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Enemy enemyScript = collision.gameObject.GetComponent<Enemy>();
            if (enemyScript != null && enemyScript.currentState == Enemy.State.Rolling)
            {
                TakeDamage();
                enemyScript.Die(true);
            }
        }
    }

    void TakeDamage()
    {
        currentHealth--;

        // EFECTO DE SONIDO: DAÑO
        if (AudioManager.instance != null)
            AudioManager.instance.PlaySFX(AudioManager.instance.sonidoDanoJefe);

        if (anim != null) anim.SetTrigger("Hit");
        sr.color = Color.red;
        Invoke("ResetColor", 0.15f);

        if (currentHealth <= 0) Die();
    }

    void ResetColor()
    {
        if (currentHealth > 0) sr.color = Color.white;
    }

    void Die()
    {
        // EFECTO DE SONIDO: VICTORIA
        if (AudioManager.instance != null)
            AudioManager.instance.PlaySFX(AudioManager.instance.sonidoVictoria);

        if (anim != null) anim.SetTrigger("Die");
        GetComponent<Collider2D>().enabled = false;

        if (copaPrefab != null)
        {
            Instantiate(copaPrefab, transform.position + Vector3.down * 1f, Quaternion.identity);
        }

        Destroy(gameObject, 2f);
    }
}