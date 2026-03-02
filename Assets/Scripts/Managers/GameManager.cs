using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    [Header("Score")]
    public int score = 0;
    public TextMeshProUGUI scoreText;

    [Header("Lives")]
    public int lives = 3;
    public TextMeshProUGUI livesText;

    [Header("Floating Text")]
    public GameObject floatingTextPrefab;

    [Header("Player")]
    public GameObject playerPrefab;
    public Transform spawnPoint;

    private GameObject currentPlayer;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        UpdateScoreUI();
        UpdateLivesUI();
        SpawnPlayer();
    }

    void SpawnPlayer()
    {
        currentPlayer = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
    }

    public void PlayerDied()
    {
        lives--;
        UpdateLivesUI();

        if (lives <= 0)
        {
            GameOver();
        }
        else
        {
            Invoke(nameof(SpawnPlayer), 1.5f);
        }
    }

    void GameOver()
    {
        UnityEngine.Debug.Log("GAME OVER");
        ShowFloatingText("GAME OVER", spawnPoint.position + Vector3.up * 2f);
        Invoke(nameof(ReloadScene), 3f);
    }

    void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void AddScore(int points)
    {
        score += points;
        UpdateScoreUI();
    }

    void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + score;
    }

    void UpdateLivesUI()
    {
        if (livesText != null)
            livesText.text = "Lives: " + lives;
    }

    public void ShowFloatingText(string text, Vector3 position)
    {
        if (floatingTextPrefab == null)
            return;

        GameObject ft = Instantiate(floatingTextPrefab, position, Quaternion.identity);

        FloatingText floating = ft.GetComponent<FloatingText>();
        if (floating != null)
            floating.SetText(text);
    }

    public void CheckLevelClear()
    {
        Invoke(nameof(VerifyEnemies), 0.1f);
    }

    void VerifyEnemies()
    {
        Enemy[] enemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        if (enemies.Length == 0)
        {
            ShowFloatingText("LEVEL CLEAR!", spawnPoint.position + Vector3.up * 3f);
            Invoke(nameof(ReloadScene), 3f);
        }
    }
}