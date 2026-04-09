using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void EmpezarPartido()
    {
        // Le avisamos al AudioManager que cambie a música de nivel
        if (AudioManager.instance != null)
        {
            AudioManager.instance.IniciarMusicaNivel();
        }

        // Cargamos la escena del juego
        SceneManager.LoadScene("MainScene");
    }

    public void SalirDelJuego()
    {
        // CAMBIO AQUÍ: Usamos el nombre completo para evitar la ambigüedad
        UnityEngine.Application.Quit();

        UnityEngine.Debug.Log("Saliendo del juego...");
    }
}