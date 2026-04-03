using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 11f;
    public GameObject snowballPrefab;
    public Transform firePoint;

    [Header("Inmunidad")]
    public float tiempoInmunidad = 2.5f; // Tiempo que serás intocable al aparecer
    private bool esInmune = false;
    private SpriteRenderer playerSR; // Para hacer el efecto de parpadeo

    private bool facingRight = true;
    private Rigidbody2D rb;
    private Animator anim; 
    private bool isGrounded;
    private bool isDead = false;

    private GameObject currentOneWayPlatform;

    public bool isTransitioning = false;
    private Vector3 targetTransitionPosition;
    private float originalGravity;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>(); 
        playerSR = GetComponent<SpriteRenderer>(); // Enlazamos el dibujo
        originalGravity = rb.gravityScale;

        // Activamos la inmunidad nada más nacer
        StartCoroutine(ActivarInmunidad());
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

        if (moveX > 0 && !facingRight) Flip();
        else if (moveX < 0 && facingRight) Flip();

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
                    anim.SetBool("isGrounded", false); 
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            Shoot();
        }
    }

    // Rutina de parpadeo de invencibilidad
    private IEnumerator ActivarInmunidad()
    {
        esInmune = true;
        float tiempoPasado = 0;
        
        // Mientras no se acabe el tiempo, parpadea
        while (tiempoPasado < tiempoInmunidad)
        {
            playerSR.enabled = !playerSR.enabled; 
            yield return new WaitForSeconds(0.15f); // Velocidad del parpadeo
            tiempoPasado += 0.15f;
        }
        
        // Al terminar, aseguramos que quede visible y sea mortal otra vez
        playerSR.enabled = true; 
        esInmune = false;
    }

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
                    anim.SetTrigger("Kick"); 
                    enemy.Kick(dir);
                    return;
                }
            }
        }

        anim.SetTrigger("Shot"); 
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
        if (isDead) return; 
        isDead = true;
        rb.linearVelocity = Vector2.zero;

        anim.SetTrigger("Die"); 

        StartCoroutine(DieRoutine()); 
    }

    private IEnumerator DieRoutine()
    {
        yield return new WaitForSeconds(1.5f); 
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
            // SI SOMOS INMUNES, IGNORAMOS EL CHOQUE Y SALIMOS DE AQUÍ
            if (esInmune) return; 

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

        anim.SetTrigger("Transition"); 

        targetTransitionPosition = new Vector3(transform.position.x, targetY, transform.position.z);
    }

    public void EndTransition()
    {
        isTransitioning = false;
        rb.gravityScale = originalGravity;
        GetComponent<Collider2D>().enabled = true;
    }
}