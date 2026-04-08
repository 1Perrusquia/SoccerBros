using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Boss jefe = UnityEngine.Object.FindFirstObjectByType<Boss>();

            if (jefe != null)
            {
                jefe.ActivateBoss();

                // CAMBIO DE MÚSICA A MODO JEFE
                if (AudioManager.instance != null)
                    AudioManager.instance.CambiarMusicaJefe();

                Destroy(gameObject);
            }
        }
    }
}