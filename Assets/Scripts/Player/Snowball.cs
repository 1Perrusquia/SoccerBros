using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Snowball : MonoBehaviour
{
    public float forwardSpeed = 8f;
    public float upwardForce = 3f;
    public float lifeTime = 1.5f;

    private Rigidbody2D rb;

    // <--- AGREGADO: Variable para recordar si este disparo es súper poderoso
    private bool isSuperPowered = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetDirection(Vector2 dir)
    {
        // Aplicamos la velocidad hacia adelante y un ligero impulso hacia arriba
        rb.linearVelocity = new Vector2(dir.x * forwardSpeed, upwardForce);

        // OJO: Quitamos el Destroy de aquí porque ahora lo calcularemos con las pociones
    }

    // =========================================================================
    // <--- AGREGADO: La función que recibe la orden desde PlayerMovement.cs
    // =========================================================================
    public void ApplyPowerUpEffects(bool hasBluePotion, bool hasYellowPotion)
    {
        isSuperPowered = hasBluePotion;

        if (hasBluePotion)
        {
            // POCIÓN AZUL: Vuelve más grande el disparo
            transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        }

        float finalLifeTime = lifeTime;

        if (hasYellowPotion)
        {
            // POCIÓN AMARILLA: Otorga más alcance (duplica el tiempo de vida del proyectil)
            finalLifeTime = lifeTime * 2.5f;
        }

        // Ahora sí, destruimos el proyectil con el tiempo calculado
        Destroy(gameObject, finalLifeTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
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

            // <--- AGREGADO: El truco del golpe doble para la Poción Azul
            if (isSuperPowered)
            {
                enemy.TakeSnowHit(); // Lo golpea de nuevo al instante para envolverlo más rápido
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(100);
                GameManager.Instance.ShowFloatingText("+100", enemy.transform.position);
            }
        }

        Destroy(gameObject);
    }
}