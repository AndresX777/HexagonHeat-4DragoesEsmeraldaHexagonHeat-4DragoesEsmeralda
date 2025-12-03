using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the hexagon game flow: rounds, victory and defeat conditions
/// </summary>
public class HexagonGameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [Tooltip("Time between color changes (rounds)")]
    [SerializeField] private float roundDuration = 5f;

    [Tooltip("Warning time before hexagons fall")]
    [SerializeField] private float warningTime = 2f;

    [Tooltip("Time to wait before starting new round")]
    [SerializeField] private float regenerationTime = 2f;

    [Tooltip("Rounds needed to win the game")]
    [SerializeField] private int roundsToWin = 10;

    [Header("Hexagon References")]
    [Tooltip("All hexagons in the scene")]
    [SerializeField] private List<HexagonController> allHexagons = new List<HexagonController>();

    [Header("UI References")]
    [Tooltip("Text to display safe color")]
    [SerializeField] private UnityEngine.UI.Text safeColorText;

    [Tooltip("UI Manager reference")]
    [SerializeField] private UIManager uiManager;

    [Header("Visual Elements")]
    [Tooltip("Bandera controller for Albion")]
    [SerializeField] private BanderaController banderaController;

    // Estados del juego
    public enum GameState
    {
        Playing,
        Victory,
        Defeat,
        Paused
    }

    // Private variables
    private HexagonColor currentSafeColor;
    private bool gameStarted = false;
    private int currentRound = 0;
    private GameState currentState = GameState.Playing;

    #region Unity Lifecycle

    private void Awake()
    {
        if (uiManager == null)
        {
            uiManager = FindObjectOfType<UIManager>();
        }
    }

    private void Start()
    {
        if (allHexagons.Count == 0)
        {
            FindAllHexagons();
        }

        StartGame();
    }

    private void OnEnable()
    {
        PlayerController.OnPlayerDied += HandlePlayerDeath;
    }

    private void OnDisable()
    {
        PlayerController.OnPlayerDied -= HandlePlayerDeath;
    }

    #endregion

    #region Game Flow

    public void StartGame()
    {
        gameStarted = true;
        currentRound = 0;
        currentState = GameState.Playing;

        Debug.Log("🎮 Hexagon Heat game started!");
        Debug.Log($"🏆 Objetivo: Sobrevivir {roundsToWin} rondas");

        StartCoroutine(GameLoop());
    }

    private IEnumerator GameLoop()
    {
        while (gameStarted && currentState == GameState.Playing)
        {
            currentRound++;

            if (currentRound > roundsToWin)
            {
                HandleVictory();
                yield break;
            }

            yield return StartCoroutine(RunRound());

            if (currentState != GameState.Playing)
            {
                yield break;
            }

            yield return new WaitForSeconds(regenerationTime);

            RegenerateAllHexagons();

            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator RunRound()
    {
        currentSafeColor = (HexagonColor)Random.Range(0, System.Enum.GetValues(typeof(HexagonColor)).Length);

        Debug.Log($"===== RONDA {currentRound}/{roundsToWin} ===== Color seguro: {currentSafeColor}");

        if (uiManager != null)
        {
            uiManager.UpdateRound(currentRound, roundsToWin);
        }

        if (safeColorText != null)
        {
            safeColorText.text = $"Ronda {currentRound}/{roundsToWin}\nColor Seguro: {currentSafeColor}";
        }

        if (banderaController != null)
        {
            banderaController.ShowFlag(currentSafeColor);
        }

        yield return new WaitForSeconds(warningTime);

        if (currentState != GameState.Playing) yield break;

        DropUnsafeHexagons();

        yield return new WaitForSeconds(roundDuration);
    }

    private void DropUnsafeHexagons()
    {
        int fallingCount = 0;

        foreach (HexagonController hex in allHexagons)
        {
            if (hex != null)
            {
                bool isSafe = (hex.GetColor() == currentSafeColor);
                hex.SetSafeState(isSafe);

                if (!isSafe)
                {
                    fallingCount++;
                }
            }
        }

        Debug.Log($"⚠️ ¡Hexágonos cayendo! Solo {currentSafeColor} es seguro. ({fallingCount} cayendo)");
    }

    private void RegenerateAllHexagons()
    {
        foreach (HexagonController hex in allHexagons)
        {
            if (hex != null)
            {
                hex.Regenerate();
            }
        }

        Debug.Log($"🔄 Hexágonos regenerados. ¡Preparado para Ronda {currentRound + 1}!");
    }

    #endregion

    #region Victory/Defeat Conditions

    /// <summary>
    /// Manejar cuando el jugador muere (cae al agua)
    /// </summary>
    private void HandlePlayerDeath()
    {
        if (currentState != GameState.Playing) return;

        currentState = GameState.Defeat;
        gameStarted = false;

        Debug.Log("💀 ¡GAME OVER! El jugador cayó al agua.");

        // Guardar resultado para la otra escena
        if (GameResult.Instance != null)
        {
            GameResult.Instance.SetLose(currentRound);
        }

        // Detener todas las corrutinas
        StopAllCoroutines();

        // Ir al menú después de un delay
        StartCoroutine(LoadMenuAfterDelay(1.5f));
    }

    /// <summary>
    /// Manejar victoria
    /// </summary>
    private void HandleVictory()
    {
        if (currentState != GameState.Playing) return;

        currentState = GameState.Victory;
        gameStarted = false;

        Debug.Log($"🏆 ¡VICTORIA! Sobreviviste {roundsToWin} rondas.");

        // Guardar resultado para la otra escena
        if (GameResult.Instance != null)
        {
            GameResult.Instance.SetWin(currentRound);
        }

        StopAllCoroutines();

        // Ir al menú después de un delay
        StartCoroutine(LoadMenuAfterDelay(1.5f));
    }

    /// <summary>
    /// Cargar menú después de un delay
    /// </summary>
    private IEnumerator LoadMenuAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        UnityEngine.SceneManagement.SceneManager.LoadScene("UI_Menu");
    }

    #endregion

    #region Public Methods

    public void RestartGame()
    {
        Debug.Log("🔄 Reiniciando juego...");
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }

    public void GoToMainMenu()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("UI_Menu");
    }

    public GameState GetCurrentState()
    {
        return currentState;
    }

    private void FindAllHexagons()
    {
        HexagonController[] hexagons = FindObjectsOfType<HexagonController>();
        allHexagons.AddRange(hexagons);
        Debug.Log($"Found {allHexagons.Count} hexagons in the scene");
    }

    public HexagonColor GetSafeColor()
    {
        return currentSafeColor;
    }

    public int GetCurrentRound()
    {
        return currentRound;
    }

    public void StopGame()
    {
        gameStarted = false;
        Debug.Log("Game stopped!");
    }

    #endregion
}