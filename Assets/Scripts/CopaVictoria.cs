using UnityEngine;
using UnityEngine.SceneManagement;

public class CopaVictoria : MonoBehaviour
{
    private bool recogida = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Solo el jugador puede reclamar la copa y solo una vez
        if (collision.CompareTag("Player") && !recogida)
        {
            recogida = true;
            Victoria();
        }
    }

    void Victoria()
    {
        // 1. Sonido de victoria triunfal
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX(AudioManager.instance.sonidoVictoria);
        }

        UnityEngine.Debug.Log("¡CAMPEONES DEL MUNDO! Copa recogida.");

        // 2. Aquí puedes hacer dos cosas:
        // A) Mostrar un Canvas de "¡Ganaste!"
        // B) Cargar la escena de créditos o volver al inicio después de 3 segundos
        Invoke("RegresarAlMenu", 3f);
    }

    void RegresarAlMenu()
    {
        SceneManager.LoadScene("Inicio");
    }
}