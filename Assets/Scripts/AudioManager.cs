using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("Fuentes de Audio")]
    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Clips de Sonido")]
    public AudioClip musicaNivel;
    public AudioClip musicaJefe;
    public AudioClip sonidoPatada;
    public AudioClip sonidoDanoJefe;
    public AudioClip sonidoVictoria;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        if (musicaNivel != null && musicSource != null)
        {
            musicSource.clip = musicaNivel;
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

    public void CambiarMusicaJefe()
    {
        if (musicSource != null && musicaJefe != null)
        {
            musicSource.Stop();
            musicSource.clip = musicaJefe;
            musicSource.Play();
        }
    }
}