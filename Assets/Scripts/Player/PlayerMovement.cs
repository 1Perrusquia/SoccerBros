using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float jumpForce = 11f;
    public GameObject snowballPrefab;
    public Transform firePoint;

<<<<<<< HEAD
    // <--- AGREGADO PARA POWER UPS: Guardamos los valores originales
    private float defaultMoveSpeed;
    private float defaultJumpForce;

    // <--- AGREGADO PARA POWER UPS: Banderas para saber qué tomamos
    [Header("Estado de Power Ups")]
    public bool hasRedPotion = false;
    public bool hasBluePotion = false;
    public bool hasYellowPotion = false;

    private bool facingRight = true;
    private Rigidbody2D rb;
    private Animator anim;
=======
    [Header("Inmunidad")]
    public float tiempoInmunidad = 2.5f; // Tiempo que serás intocable al aparecer
    private bool esInmune = false;
    private SpriteRenderer playerSR; // Para hacer el efecto de parpadeo

    private bool facingRight = true;
    private Rigidbody2D rb;
    private Animator anim; 
>>>>>>> e8e6e76e7382f5eda3cf9fbedbe6e790466f1eb0
    private bool isGrounded;
    private bool isDead = false;

    private GameObject currentOneWayPlatform;

    public bool isTransitioning = false;
    private Vector3 targetTransitionPosition;
    private float originalGravity;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
<<<<<<< HEAD
        anim = GetComponent<Animator>();
        originalGravity = rb.gravityScale;

        // <--- AGREGADO PARA POWER UPS: Guardamos la velocidad normal al iniciar el nivel
        defaultMoveSpeed = moveSpeed;
        defaultJumpForce = jumpForce;
=======
        anim = GetComponent<Animator>(); 
        playerSR = GetComponent<SpriteRenderer>(); // Enlazamos el dibujo
        originalGravity = rb.gravityScale;

        // Activamos la inmunidad nada más nacer
        StartCoroutine(ActivarInmunidad());
>>>>>>> e8e6e76e7382f5eda3cf9fbedbe6e790466f1eb0
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

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

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
<<<<<<< HEAD
                    anim.SetBool("isGrounded", false);
=======
                    anim.SetBool("isGrounded", false); 
>>>>>>> e8e6e76e7382f5eda3cf9fbedbe6e790466f1eb0
                    rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                }
            }
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            Shoot();
        }
    }

<<<<<<< HEAD
=======
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

>>>>>>> e8e6e76e7382f5eda3cf9fbedbe6e790466f1eb0
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
<<<<<<< HEAD
                    anim.SetTrigger("Kick");
=======
                    anim.SetTrigger("Kick"); 
>>>>>>> e8e6e76e7382f5eda3cf9fbedbe6e790466f1eb0
                    enemy.Kick(dir);
                    return;
                }
            }
        }

<<<<<<< HEAD
        anim.SetTrigger("Shot");
=======
        anim.SetTrigger("Shot"); 
>>>>>>> e8e6e76e7382f5eda3cf9fbedbe6e790466f1eb0
        GameObject snowball = Instantiate(snowballPrefab, firePoint.position, Quaternion.identity);
        Vector2 dirShoot = facingRight ? Vector2.right : Vector2.left;

        Snowball snowballScript = snowball.GetComponent<Snowball>();
        snowballScript.SetDirection(dirShoot);

        // <--- AGREGADO PARA POWER UPS: Le avisamos a la bola de nieve qué pociones tenemos
        // (Nota: Tendremos que agregar esta función en tu script 'Snowball.cs' después)
        snowballScript.ApplyPowerUpEffects(hasBluePotion, hasYellowPotion);
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
<<<<<<< HEAD
        if (isDead) return;
        isDead = true;
        rb.linearVelocity = Vector2.zero;

        anim.SetTrigger("Die");

        // <--- AGREGADO PARA POWER UPS: Al morir, perdemos todos los poderes y volvemos a la normalidad
        hasRedPotion = false;
        hasBluePotion = false;
        hasYellowPotion = false;
        moveSpeed = defaultMoveSpeed;
        jumpForce = defaultJumpForce;
        anim.SetBool("isFastRun", false); // Apagamos la animación de correr rápido

        StartCoroutine(DieRoutine());
=======
        if (isDead) return; 
        isDead = true;
        rb.linearVelocity = Vector2.zero;

        anim.SetTrigger("Die"); 

        StartCoroutine(DieRoutine()); 
>>>>>>> e8e6e76e7382f5eda3cf9fbedbe6e790466f1eb0
    }

    private IEnumerator DieRoutine()
    {
<<<<<<< HEAD
        yield return new WaitForSeconds(1.5f);
=======
        yield return new WaitForSeconds(1.5f); 
>>>>>>> e8e6e76e7382f5eda3cf9fbedbe6e790466f1eb0
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

<<<<<<< HEAD
        anim.SetTrigger("Transition");
=======
        anim.SetTrigger("Transition"); 
>>>>>>> e8e6e76e7382f5eda3cf9fbedbe6e790466f1eb0

        targetTransitionPosition = new Vector3(transform.position.x, targetY, transform.position.z);
    }

    public void EndTransition()
    {
        isTransitioning = false;
        rb.gravityScale = originalGravity;
        GetComponent<Collider2D>().enabled = true;
    }

    // =========================================================================
    // <--- AGREGADO PARA POWER UPS: La función que recibe el ítem cuando lo tocas
    // =========================================================================
    public void ApplyPowerUp(PowerUp.Type type, int score)
    {
        switch (type)
        {
            case PowerUp.Type.Roja:
                hasRedPotion = true;
                moveSpeed = defaultMoveSpeed * 1.5f; // 50% más rápido
                jumpForce = defaultJumpForce * 1.2f; // Salta 20% más alto
                anim.SetBool("isFastRun", true); // Le avisa al Animator que use la nueva animación
                break;

            case PowerUp.Type.Azul:
                hasBluePotion = true;
                // La lógica de fuerza se aplicará al momento de disparar en la función Shoot()
                break;

            case PowerUp.Type.Amarilla:
                hasYellowPotion = true;
                // La lógica de distancia se aplicará al momento de disparar en la función Shoot()
                break;

            case PowerUp.Type.Comida:
                // Sumamos puntos a tu GameManager
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.AddScore(score);
                }
                break;
        }
    }
}