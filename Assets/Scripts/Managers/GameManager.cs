using System.Diagnostics;
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

    // --- VARIABLES DE TRANSICIÓN ---
    [Header("Level Transition")]
    public Transform mainCamera;
    public float cameraHeight = 10f;
    public float transitionSpeed = 5f;

    // Ahora es pública para que FloatingText pueda saber cuándo pausarse
    [HideInInspector]
    public bool isTransitioningLevel = false;

    private Vector3 cameraTargetPosition;
    private GameObject currentPlayer;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);

        // --- SOLUCIÓN AL ERROR ---
        // Si olvidaste asignar la cámara en el Inspector, Unity la busca automáticamente.
        if (mainCamera == null)
        {
            if (Camera.main != null)
            {
                mainCamera = Camera.main.transform;
            }
            else
            {
                UnityEngine.Debug.Log("¡No hay ninguna cámara etiquetada como 'MainCamera' en la escena!");
            }
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
            mainCamera.position = Vector3.MoveTowards(mainCamera.position, cameraTargetPosition, transitionSpeed * Time.deltaTime);

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
        // Encontramos a todos los enemigos de toda la escena
        Enemy[] allEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        int enemiesInCurrentScreen = 0;

        // Calculamos los límites del nivel actual basándonos en la altura de la cámara
        float currentCameraY = mainCamera.position.y;
        float halfCameraHeight = cameraHeight / 2f; // Mitad de la altura para calcular arriba y abajo

        foreach (Enemy enemy in allEnemies)
        {
            // Solo contamos a los enemigos que están en el rango de visión vertical de la cámara actual
            // Le damos un margen extra de 1f por si algún enemigo salta o está ligeramente fuera
            if (enemy.transform.position.y < (currentCameraY + halfCameraHeight + 1f) &&
                enemy.transform.position.y > (currentCameraY - halfCameraHeight - 1f))
            {
                enemiesInCurrentScreen++;
            }
        }

        // Ahora comprobamos si limpiamos la pantalla actual
        if (enemiesInCurrentScreen == 0 && !isTransitioningLevel)
        {
            ShowFloatingText("LEVEL CLEAR!", spawnPoint.position + Vector3.up * 3f);
            Invoke(nameof(StartLevelTransition), 2f); // Esperamos 2 segundos antes de subir
        }
    }

    void StartLevelTransition()
    {
        isTransitioningLevel = true;
        cameraTargetPosition = mainCamera.position + new Vector3(0, cameraHeight, 0);
        spawnPoint.position += new Vector3(0, cameraHeight, 0);

        if (currentPlayer != null)
        {
            float playerNewY = currentPlayer.transform.position.y + cameraHeight;
            currentPlayer.GetComponent<PlayerMovement>().StartTransitionToNextLevel(playerNewY);
        }
    }

    void FinishLevelTransition()
    {
        isTransitioningLevel = false;

        if (currentPlayer != null)
        {
            currentPlayer.GetComponent<PlayerMovement>().EndTransition();
        }
    }
}