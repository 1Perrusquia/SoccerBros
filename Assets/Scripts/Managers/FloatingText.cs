using UnityEngine;
using TMPro;

public class FloatingText : MonoBehaviour
{
    public float moveSpeed = 2f;
    public float duration = 1f;

    private TextMeshPro textMesh;
    private Color originalColor;
    private float timer;

    void Awake()
    {
        textMesh = GetComponent<TextMeshPro>();
        originalColor = textMesh.color;
    }

    void Update()
    {
        // Movimiento hacia arriba
        transform.position += Vector3.up * moveSpeed * Time.deltaTime;

        // Timer
        timer += Time.deltaTime;

        // Fade out
        float alpha = Mathf.Lerp(1f, 0f, timer / duration);
        textMesh.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

        // Destroy
        if (timer >= duration)
        {
            Destroy(gameObject);
        }
    }

    public void SetText(string text)
    {
        textMesh.text = text;
    }
}