using TMPro;
using UnityEngine;
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

    [Header("Level Transition")]
    public Transform mainCamera;
    public float cameraHeight = 10f;
    public float transitionSpeed = 5f;

    [HideInInspector]
    public bool isTransitioningLevel = false;

    private Vector3 cameraTargetPosition;
    private GameObject currentPlayer;

    void Awake()
    {
        // Singleton pattern
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return; // Importante para evitar que el código siga ejecutándose en la instancia destruida
        }

        if (mainCamera == null)
        {
            if (Camera.main != null)
                mainCamera = Camera.main.transform;
            else
                UnityEngine.Debug.LogError("¡No hay ninguna cámara etiquetada como 'MainCamera'!");
        }
    }

    void Start()
    {
        UpdateScoreUI();
        UpdateLivesUI();
        SpawnPlayer();
    }

    void Update()
    {
        if (isTransitioningLevel && mainCamera != null)
        {
            // Movimiento suave de la cámara hacia el objetivo
            mainCamera.position = Vector3.MoveTowards(mainCamera.position, cameraTargetPosition, transitionSpeed * Time.deltaTime);

            if (Vector3.Distance(mainCamera.position, cameraTargetPosition) < 0.01f)
            {
                // Aseguramos la posición final exacta
                mainCamera.position = cameraTargetPosition;
                FinishLevelTransition();
            }
        }
    }

    void SpawnPlayer()
    {
        // Limpieza de seguridad por si ya existe un jugador
        if (currentPlayer != null) Destroy(currentPlayer);

        currentPlayer = Instantiate(playerPrefab, spawnPoint.position, Quaternion.identity);
    }

    public void PlayerDied()
    {
        lives--;
        UpdateLivesUI();

        if (lives <= 0)
            GameOver();
        else
            Invoke(nameof(SpawnPlayer), 1.5f);
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
        if (floatingTextPrefab == null) return;

        GameObject ft = Instantiate(floatingTextPrefab, position, Quaternion.identity);
        // Nota: Asegúrate de que el script en el prefab se llame exactamente 'FloatingText'
        FloatingText floating = ft.GetComponent<FloatingText>();
        if (floating != null)
            floating.SetText(text);
    }

    public void CheckLevelClear()
    {
        // Cancelamos invocaciones previas para evitar que se ejecute múltiples veces si mueren 2 enemigos a la vez
        CancelInvoke(nameof(VerifyEnemies));
        Invoke(nameof(VerifyEnemies), 0.2f);
    }

    void VerifyEnemies()
    {
        // Optimizamos: Si ya estamos cambiando de nivel, no tiene sentido contar
        if (isTransitioningLevel) return;

        Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        int enemiesInCurrentScreen = 0;

        float currentCameraY = mainCamera.position.y;
        float halfHeight = cameraHeight / 2f;

        foreach (Enemy enemy in allEnemies)
        {
            // CORRECCIÓN: Verificamos que el enemigo no sea nulo (por si se destruyó en este frame)
            if (enemy == null) continue;

            float enemyY = enemy.transform.position.y;
            // Solo contamos enemigos dentro de la "franja" actual de la cámara
            if (enemyY < (currentCameraY + halfHeight) && enemyY > (currentCameraY - halfHeight))
            {
                enemiesInCurrentScreen++;
            }
        }

        if (enemiesInCurrentScreen == 0)
        {
            ShowFloatingText("LEVEL CLEAR!", spawnPoint.position + Vector3.up * 3f);
            Invoke(nameof(StartLevelTransition), 2f);
        }
    }

    void StartLevelTransition()
    {
        if (isTransitioningLevel) return;

        isTransitioningLevel = true;
        cameraTargetPosition = mainCamera.position + new Vector3(0, cameraHeight, 0);

        // El nuevo punto de spawn ahora está un piso arriba
        spawnPoint.position += new Vector3(0, cameraHeight, 0);

        if (currentPlayer != null)
        {
            // CORRECCIÓN: Verificamos que el componente existe antes de llamar a la función
            var movement = currentPlayer.GetComponent<PlayerMovement>();
            if (movement != null)
            {
                float playerNewY = currentPlayer.transform.position.y + cameraHeight;
                movement.StartTransitionToNextLevel(playerNewY);
            }
        }
    }

    void FinishLevelTransition()
    {
        isTransitioningLevel = false;

        if (currentPlayer != null)
        {
            var movement = currentPlayer.GetComponent<PlayerMovement>();
            if (movement != null)
                movement.EndTransition();
        }
    }
}