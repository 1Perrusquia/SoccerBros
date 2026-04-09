using UnityEngine;
using UnityEngine.SceneManagement; // Agregamos esto para detectar cambios de escena

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Fuentes de Audio")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Clips de Música")]
    public AudioClip musicaMenu;
    public AudioClip musicaNivel;
    public AudioClip musicaJefe;

    [Header("Clips de Efectos (SFX)")]
    public AudioClip sonidoPatada;
    public AudioClip sonidoDanoJefe;
    public AudioClip sonidoVictoria;
    public AudioClip sonidoDieJugador;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            // Nos suscribimos al evento de carga de escena
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Esta función se ejecuta CADA VEZ que cambia la escena
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Si entramos a la escena de Inicio o Menu
        if (scene.name == "Inicio" || scene.name == "Menu")
        {
            ReproducirMusica(musicaMenu);
        }
    }

    void ReproducirMusica(AudioClip clip)
    {
        if (musicSource != null && clip != null)
        {
            if (musicSource.clip == clip) return; // Si ya está sonando, no la reinicies

            musicSource.Stop();
            musicSource.clip = clip;
            musicSource.loop = true;
            musicSource.Play();
        }
    }

    public void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }

    public void IniciarMusicaNivel()
    {
        ReproducirMusica(musicaNivel);
    }

    public void CambiarMusicaJefe()
    {
        ReproducirMusica(musicaJefe);
    }

    private void OnDestroy()
    {
        // Limpiamos el evento al destruir el objeto para evitar errores
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}