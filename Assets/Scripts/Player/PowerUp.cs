using UnityEngine;

public class PowerUp : MonoBehaviour
{
    // Creamos una lista de tipos para elegir desde el Inspector de Unity
    public enum Type { Roja, Azul, Amarilla, Comida }
    public Type powerUpType;

    public int scoreValue = 500; // Para la comida mexicana

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Si lo que nos tocó es el jugador...
        if (collision.CompareTag("Player"))
        {
            // Buscamos su script y le mandamos el poder
            PlayerMovement player = collision.GetComponent<PlayerMovement>();
            if (player != null)
            {
                player.ApplyPowerUp(powerUpType, scoreValue);
            }

            // Destruimos la poción de la pantalla
            Destroy(gameObject);
        }
    }
}