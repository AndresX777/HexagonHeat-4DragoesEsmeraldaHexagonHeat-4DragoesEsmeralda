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
    [SerializeField] private int roundsToWin = 10; // ⭐ NUEVO

    [Header("Hexagon References")]
    [Tooltip("All hexagons in the scene")]
    [SerializeField] private List<HexagonController> allHexagons = new List<HexagonController>();

    [Header("UI References")]
    [Tooltip("Text to display safe color")]
    [SerializeField] private UnityEngine.UI.Text safeColorText;

    [Tooltip("UI Manager reference")]
    [SerializeField] private UIManager uiManager; // ⭐ NUEVO

    [Header("Visual Elements")]
    [Tooltip("Bandera controller for Albion")]
    [SerializeField] private BanderaController banderaController;

    // ⭐ NUEVO: Estados del juego
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
    private GameState currentState = GameState.Playing; // ⭐ NUEVO

    #region Unity Lifecycle

    private void Awake()
    {
        // ⭐ Buscar UIManager si no está asignado
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
        // ⭐ SUSCRIBIRSE al evento de muerte del jugador
        PlayerController.OnPlayerDied += HandlePlayerDeath;
    }

    private void OnDisable()
    {
        // ⭐ DESUSCRIBIRSE del evento
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
            // Start new round
            currentRound++;

            // ⭐ VERIFICAR VICTORIA por rondas
            if (currentRound > roundsToWin)
            {
                HandleVictory();
                yield break; // Salir del loop
            }

            yield return StartCoroutine(RunRound());

            // Verificar si el juego sigue activo
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
        // Select random safe color
        currentSafeColor = (HexagonColor)Random.Range(0, System.Enum.GetValues(typeof(HexagonColor)).Length);

        Debug.Log($"===== RONDA {currentRound}/{roundsToWin} ===== Color seguro: {currentSafeColor}");

        // Update UI
        if (uiManager != null)
        {
            uiManager.UpdateRound(currentRound, roundsToWin); // ⭐ MODIFICADO
        }

        if (safeColorText != null)
        {
            safeColorText.text = $"Ronda {currentRound}/{roundsToWin}\nColor Seguro: {currentSafeColor}";
        }

        // Show flag
        if (banderaController != null)
        {
            banderaController.ShowFlag(currentSafeColor);
        }

        yield return new WaitForSeconds(warningTime);

        // Verificar estado antes de hacer caer hexágonos
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
    /// ⭐ NUEVO: Manejar cuando el jugador muere (cae al agua)
    /// </summary>
    private void HandlePlayerDeath()
    {
        if (currentState != GameState.Playing) return;

        currentState = GameState.Defeat;
        gameStarted = false;

        Debug.Log("💀 ¡GAME OVER! El jugador cayó al agua.");

        // Mostrar UI de derrota
        if (uiManager != null)
        {
            uiManager.ShowGameOver(currentRound, roundsToWin);
        }

        // Detener todas las corrutinas
        StopAllCoroutines();
    }

    /// <summary>
    /// ⭐ NUEVO: Manejar victoria
    /// </summary>
    private void HandleVictory()
    {
        if (currentState != GameState.Playing) return;

        currentState = GameState.Victory;
        gameStarted = false;

        Debug.Log($"🏆 ¡VICTORIA! Sobreviviste {roundsToWin} rondas.");

        // Mostrar UI de victoria
        if (uiManager != null)
        {
            uiManager.ShowVictory(roundsToWin);
        }

        StopAllCoroutines();
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// ⭐ NUEVO: Reiniciar el juego
    /// </summary>
    public void RestartGame()
    {
        Debug.Log("🔄 Reiniciando juego...");

        // Recargar la escena actual
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }

    /// <summary>
    /// ⭐ NUEVO: Salir al menú principal
    /// </summary>
    public void GoToMainMenu()
    {
        // Cambiar esto al nombre/índice de tu escena de menú
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }

    /// <summary>
    /// ⭐ NUEVO: Obtener estado actual del juego
    /// </summary>
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