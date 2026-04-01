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

    // --- VARIABLES DE TRANSICIÓN ---
    [Header("Level Transition")]
    public Transform mainCamera;
    public float cameraHeight = 10f;
    public float transitionSpeed = 5f;

    private bool isTransitioningLevel = false;
    private Vector3 cameraTargetPosition;
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

    void Update()
    {
        // Movimiento de la cámara durante la transición
        if (isTransitioningLevel && mainCamera != null)
        {
            mainCamera.position = Vector3.MoveTowards(mainCamera.position, cameraTargetPosition, transitionSpeed * Time.deltaTime);

            // Terminamos la transición cuando la cámara llega a su destino
            if (Vector3.Distance(mainCamera.position, cameraTargetPosition) < 0.01f)
            {
                FinishLevelTransition();
            }
        }
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

        // Agregamos !isTransitioningLevel para que no se llame varias veces
        if (enemies.Length == 0 && !isTransitioningLevel)
        {
            ShowFloatingText("LEVEL CLEAR!", spawnPoint.position + Vector3.up * 3f);
            Invoke(nameof(StartLevelTransition), 2f); // Esperamos 2 segundos antes de subir
        }
    }

    // --- MÉTODOS DE TRANSICIÓN ---
    void StartLevelTransition()
    {
        isTransitioningLevel = true;

        // Calculamos la nueva posición de la cámara
        cameraTargetPosition = mainCamera.position + new Vector3(0, cameraHeight, 0);

        // Actualizamos el spawnPoint para que reviva en el nuevo nivel si muere
        spawnPoint.position += new Vector3(0, cameraHeight, 0);

        if (currentPlayer != null)
        {
            // Calculamos a dónde debe flotar el jugador
            float playerNewY = currentPlayer.transform.position.y + cameraHeight;
            currentPlayer.GetComponent<PlayerMovement>().StartTransitionToNextLevel(playerNewY);
        }
    }

    void FinishLevelTransition()
    {
        isTransitioningLevel = false;

        // Le devolvemos el control al jugador
        if (currentPlayer != null)
        {
            currentPlayer.GetComponent<PlayerMovement>().EndTransition();
        }
    }
}