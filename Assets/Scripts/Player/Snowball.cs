using UnityEngine;

public class Snowball : MonoBehaviour
{
    public float speed = 8f;
    private Vector2 direction;

    public void SetDirection(Vector2 dir)
    {
        direction = dir.normalized;
        Destroy(gameObject, 3f);
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag("Enemy"))
            return;

        Enemy enemy = collision.GetComponent<Enemy>();

        if (enemy == null)
            return;

        if (enemy.currentState == Enemy.State.Walking)
        {
            enemy.TakeSnowHit();

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddScore(100);
                GameManager.Instance.ShowFloatingText("+100", enemy.transform.position);
            }
        }

        Destroy(gameObject);
    }
}