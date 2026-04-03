using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 11f;
    public GameObject snowballPrefab;
    public Transform firePoint;

    private bool facingRight = true;
    private Rigidbody2D rb;
    private Animator anim; // <--- AGREGADO: La variable de nuestro cerebro
    private bool isGrounded;
    private bool isDead = false;

    private GameObject currentOneWayPlatform;

    public bool isTransitioning = false;
    private Vector3 targetTransitionPosition;
    private float originalGravity;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>(); // <--- AGREGADO: Enlazamos el componente al iniciar
        originalGravity = rb.gravityScale;
    }

    void Update()
    {
        if (isDead) return;

        if (isTransitioning)
        {
            rb.linearVelocity = Vector2.zero;
            transform.position = Vector3.MoveTowards(transform.position, targetTransitionPosition, moveSpeed * Time.deltaTime);
            return;
        }

        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);

        // <--- AGREGADO: Lógica para voltear el dibujo visualmente
        if (moveX > 0 && !facingRight) Flip();
        else if (moveX < 0 && facingRight) Flip();

        // <--- AGREGADO: Le mandamos la velocidad y si toca el suelo al Animator en tiempo real
        anim.SetFloat("Speed", Mathf.Abs(moveX));
        anim.SetBool("isGrounded", isGrounded);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded)
            {
                if (moveY < -0.1f && currentOneWayPlatform != null)
                {
                    StartCoroutine(DisableCollision());
                }
                else
                {
                    isGrounded = false;
                    anim.SetBool("isGrounded", false); // <--- AGREGADO: Le avisa al cerebro al instante para reaccionar rápido
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            Shoot();
        }
    }

    // <--- AGREGADO: Función que voltea el sprite del ajolote hacia la izquierda/derecha
    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    void Shoot()
    {
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
                    anim.SetTrigger("Kick"); // <--- AGREGADO: Activa la animación de la patada
                    enemy.Kick(dir);
                    return;
                }
            }
        }

        anim.SetTrigger("Shot"); // <--- AGREGADO: Activa la animación de escupir/disparar
        GameObject snowball = Instantiate(snowballPrefab, firePoint.position, Quaternion.identity);
        Vector2 dirShoot = facingRight ? Vector2.right : Vector2.left;
        snowball.GetComponent<Snowball>().SetDirection(dirShoot);
    }

    private IEnumerator DisableCollision()
    {
        Collider2D platformCollider = currentOneWayPlatform.GetComponent<Collider2D>();
        Collider2D playerCollider = GetComponent<Collider2D>();

        if (platformCollider != null)
        {
            Physics2D.IgnoreCollision(playerCollider, platformCollider, true);
            yield return new WaitForSeconds(0.4f);
            Physics2D.IgnoreCollision(playerCollider, platformCollider, false);
        }
    }

    void Die()
    {
        if (isDead) return; // Evita morir dos veces
        isDead = true;
        rb.linearVelocity = Vector2.zero;

        anim.SetTrigger("Die"); // <--- AGREGADO: Dispara la animación de muerte

        StartCoroutine(DieRoutine()); // <--- AGREGADO: Cambiado a corrutina para darle tiempo a la animación
    }

    // <--- AGREGADO: Espera un momento antes de desaparecer al jugador
    private IEnumerator DieRoutine()
    {
        yield return new WaitForSeconds(1.5f); // Espera segundo y medio
        gameObject.SetActive(false);

        if (GameManager.Instance != null)
            GameManager.Instance.PlayerDied();
    }

    void EvaluateCollision(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground") || collision.gameObject.CompareTag("Platform"))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    isGrounded = true;
                    if (collision.gameObject.CompareTag("Platform"))
                    {
                        currentOneWayPlatform = collision.gameObject;
                    }
                    return;
                }
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        EvaluateCollision(collision);

        if (collision.gameObject.CompareTag("Enemy"))
        {
            Enemy enemy = collision.gameObject.GetComponent<Enemy>();
            if (enemy != null && enemy.currentState == Enemy.State.Walking)
            {
                Die();
            }
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        EvaluateCollision(collision);
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

    public void StartTransitionToNextLevel(float targetY)
    {
        isTransitioning = true;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;

        anim.SetTrigger("Transition"); // <--- AGREGADO: Dispara la animación de aparición/transición

        targetTransitionPosition = new Vector3(transform.position.x, targetY, transform.position.z);
    }

    public void EndTransition()
    {
        isTransitioning = false;
        rb.gravityScale = originalGravity;
        GetComponent<Collider2D>().enabled = true;
    }
}