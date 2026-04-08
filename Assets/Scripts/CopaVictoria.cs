using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class CopaVictoria : MonoBehaviour
{
    [Header("Configuracion Final")]
    public GameObject panelVictoria; // Arrastra el Panel aqui
    public float tiempoEspera = 5f;  // Cuanto tiempo se queda el texto antes de reiniciar

    private bool yaGano = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Si el jugador toca la copa y aun no hemos procesado la victoria
        if (other.CompareTag("Player") && !yaGano)
        {
            yaGano = true;
            IniciarSecuenciaVictoria();
        }
    }

    void IniciarSecuenciaVictoria()
    {
        // 1. Mostramos el letrero
        if (panelVictoria != null)
        {
            panelVictoria.SetActive(true);
        }

        // 2. Sonido de Victoria (Opcional si ya lo tienes en el Boss)
        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlaySFX(AudioManager.instance.sonidoVictoria);
        }

        // 3. Lanzamos la rutina para reiniciar
        StartCoroutine(ReiniciarJuego());
    }

    IEnumerator ReiniciarJuego()
    {
        // Esperamos unos segundos para que el jugador celebre
        yield return new WaitForSeconds(tiempoEspera);

        // Cargamos el Menu Principal (Asegurate que tu escena se llame "Menu")
        SceneManager.LoadScene("Menu");
    }
}