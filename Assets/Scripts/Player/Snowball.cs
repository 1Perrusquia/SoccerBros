using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Snowball : MonoBehaviour
{
    public float forwardSpeed = 8f;
    public float upwardForce = 3f; // La fuerza que crea la "parábola" al salir
    public float lifeTime = 1.5f;  // Duración corta típica de los arcades

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetDirection(Vector2 dir)
    {
        // Aplicamos la velocidad hacia adelante y un ligero impulso hacia arriba
        rb.linearVelocity = new Vector2(dir.x * forwardSpeed, upwardForce);

        // Destruimos el proyectil rápido para limitar el rango de ataque
        Destroy(gameObject, lifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Si choca con el piso o paredes, se destruye al instante (típico de Snow Bros)
        if (collision.CompareTag("Ground") || collision.CompareTag("Platform") || collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
            return;
        }

        if (!collision.CompareTag("Enemy"))
            return;

        Enemy enemy = collision.GetComponent<Enemy>();

        if (enemy == null)
            return;

        if (enemy.currentState == Enemy.State.Walking || enemy.currentState == Enemy.State.Ball)
        {
            enemy.TakeSnowHit();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(100);
                GameManager.Instance.ShowFloatingText("+100", enemy.transform.position);
            }
        }

        Destroy(gameObject); // El balón se deshace al golpear al enemigo
    }
}