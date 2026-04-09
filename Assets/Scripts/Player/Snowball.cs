using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Snowball : MonoBehaviour
{
    [Header("Configuracion de Lanzamiento")]
    public float forwardSpeed = 8f;
    public float upwardForce = 3f;
    public float lifeTime = 1.5f;

    private Rigidbody2D rb;
    private bool isSuperPowered = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetDirection(Vector2 dir)
    {
        // Aplicamos velocidad inicial con un pequeño arco hacia arriba
        rb.linearVelocity = new Vector2(dir.x * forwardSpeed, upwardForce);
    }

    public void ApplyPowerUpEffects(bool hasBluePotion, bool hasYellowPotion)
    {
        isSuperPowered = hasBluePotion;

        if (hasBluePotion)
        {
            // POCIÓN AZUL: Aumenta el tamaño visual del proyectil
            transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        }

        float finalLifeTime = lifeTime;

        if (hasYellowPotion)
        {
            // POCIÓN AMARILLA: Alcance mucho mayor
            finalLifeTime = lifeTime * 2.5f;
        }

        Destroy(gameObject, finalLifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Chocar con el entorno
        if (collision.CompareTag("Ground") || collision.CompareTag("Platform") || collision.CompareTag("Wall"))
        {
            Destroy(gameObject);
            return;
        }

        // Chocar con un enemigo
        if (collision.CompareTag("Enemy"))
        {
            Enemy enemy = collision.GetComponent<Enemy>();

            if (enemy != null)
            {
                // Solo golpeamos si el enemigo está caminando o ya es bola (para rellenarlo más)
                if (enemy.currentState == Enemy.State.Walking || enemy.currentState == Enemy.State.Ball)
                {
                    enemy.TakeSnowHit();

                    // EFECTO POCIÓN AZUL: Golpe doble instantáneo
                    if (isSuperPowered)
                    {
                        enemy.TakeSnowHit();
                    }

                    // Sumar puntos a través del GameManager
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.AddScore(100);
                        GameManager.Instance.ShowFloatingText("+100", enemy.transform.position);
                    }
                }
            }
            // El proyectil siempre se destruye al tocar un enemigo
            Destroy(gameObject);
        }
    }
}