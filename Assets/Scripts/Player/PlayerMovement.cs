using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    public GameObject snowballPrefab;
    public Transform firePoint;

    private bool facingRight = true;
    private Rigidbody2D rb;
    private bool isGrounded;
    private bool isDead = false;

    // Referencia a la plataforma actual para dejarse caer
    private GameObject currentOneWayPlatform;

    // --- VARIABLES DE TRANSICIÓN ---
    public bool isTransitioning = false;
    private Vector3 targetTransitionPosition;
    private float originalGravity;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        originalGravity = rb.gravityScale; // Guardamos la gravedad configurada
    }

    void Update()
    {
        if (isDead) return;

        // Bloqueamos el control y movemos al jugador hacia arriba si está en transición
        if (isTransitioning)
        {
            rb.linearVelocity = Vector2.zero;
            transform.position = Vector3.MoveTowards(transform.position, targetTransitionPosition, moveSpeed * Time.deltaTime);
            return;
        }

        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical"); // Captura arriba/abajo

        rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);

        if (moveX > 0) facingRight = true;
        if (moveX < 0) facingRight = false;

        // Lógica de Salto y Dejarse Caer
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                if (moveY < -0.1f && currentOneWayPlatform != null)
                {
                    // Dejarse caer: Abajo + Espacio
                    StartCoroutine(DisableCollision());
                }
                else
                {
                    // Salto normal
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            Shoot();
        }
    }

    void Shoot()
    {
        // Lógica de patear balones
        float kickRadius = 1f;
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, kickRadius);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Enemy"))
            {
                Enemy enemy = hit.GetComponent<Enemy>();
                if (enemy != null && enemy.currentState == Enemy.State.Ball)
                {
                    int dir = facingRight ? 1 : -1;
                    enemy.Kick(dir);
                    return;
                }
            }
        }

        // Lógica de disparar
        GameObject snowball = Instantiate(snowballPrefab, firePoint.position, Quaternion.identity);
        Vector2 dirShoot = facingRight ? Vector2.right : Vector2.left;
        snowball.GetComponent<Snowball>().SetDirection(dirShoot);
    }

    private IEnumerator DisableCollision()
    {
        BoxCollider2D platformCollider = currentOneWayPlatform.GetComponent<BoxCollider2D>();
        Collider2D playerCollider = GetComponent<Collider2D>();

        if (platformCollider != null)
        {
            Physics2D.IgnoreCollision(playerCollider, platformCollider, true);
            yield return new WaitForSeconds(0.3f); // Tiempo suficiente para atravesarla hacia abajo
            Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
        }
    }

    void Die()
    {
        isDead = true;
        rb.linearVelocity = Vector2.zero;
        gameObject.SetActive(false);

        if (GameManager.Instance != null)
            GameManager.Instance.PlayerDied();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Platform"))
        {
            isGrounded = true;
        }

        if (collision.gameObject.CompareTag("Platform"))
        {
            currentOneWayPlatform = collision.gameObject;
        }

        if (collision.gameObject.CompareTag("Enemy"))
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy != null && enemy.currentState == Enemy.State.Walking)
            {
                Die();
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Platform"))
        {
            isGrounded = false;
        }

        if (collision.gameObject.CompareTag("Platform"))
        {
            currentOneWayPlatform = null;
        }
    }

    // --- MÉTODOS DE TRANSICIÓN ---
    public void StartTransitionToNextLevel(float targetY)
    {
        isTransitioning = true;
        rb.gravityScale = 0f; // Quitamos gravedad para que flote
        GetComponent<Collider2D>().enabled = false; // Desactivamos colisiones

        // Mantiene su X actual, pero sube a la nueva Y
        targetTransitionPosition = new Vector3(transform.position.x, targetY, transform.position.z);
    }

    public void EndTransition()
    {
        isTransitioning = false;
        rb.gravityScale = originalGravity; // Regresamos la gravedad a la normalidad
        GetComponent<Collider2D>().enabled = true; // Reactivamos colisiones
    }
}