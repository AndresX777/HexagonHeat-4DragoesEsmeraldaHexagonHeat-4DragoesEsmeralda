using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages all UI elements: rounds, victory, defeat screens
/// </summary>
public class UIManager : MonoBehaviour
{
    [Header("In-Game UI")]
    [Tooltip("Text showing current round")]
    public TextMeshProUGUI roundText;

    [Tooltip("Text showing safe color")]
    public TextMeshProUGUI safeColorText;

    [Header("Game Over Panel")]
    [Tooltip("Panel that shows when player loses")]
    [SerializeField] private GameObject gameOverPanel;

    [Tooltip("Text showing defeat message")]
    [SerializeField] private TextMeshProUGUI gameOverText;

    [Header("Victory Panel")]
    [Tooltip("Panel that shows when player wins")]
    [SerializeField] private GameObject victoryPanel;

    [Tooltip("Text showing victory message")]
    [SerializeField] private TextMeshProUGUI victoryText;

    [Header("Buttons")]
    [SerializeField] private Button restartButton;
    [SerializeField] private Button menuButton;

    // Reference to game manager
    private HexagonGameManager gameManager;

    private void Awake()
    {
        gameManager = FindObjectOfType<HexagonGameManager>();

        // Ocultar paneles al inicio
        if (gameOverPanel != null) gameOverPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);

        // Configurar botones
        SetupButtons();
    }

    private void SetupButtons()
    {
        if (restartButton != null)
        {
            restartButton.onClick.AddListener(OnRestartClicked);
        }

        if (menuButton != null)
        {
            menuButton.onClick.AddListener(OnMenuClicked);
        }
    }

    #region Round Updates

    /// <summary>
    /// Update round display (simple version)
    /// </summary>
    public void UpdateRound(int roundNumber)
    {
        if (roundText != null)
        {
            roundText.text = $"Ronda {roundNumber}";
        }
    }

    /// <summary>
    /// Update round display with total rounds
    /// </summary>
    public void UpdateRound(int currentRound, int totalRounds)
    {
        if (roundText != null)
        {
            roundText.text = $"Ronda {currentRound} / {totalRounds}";
        }
    }

    /// <summary>
    /// Update safe color display
    /// </summary>
    public void UpdateSafeColor(string colorName, Color displayColor)
    {
        if (safeColorText != null)
        {
            safeColorText.text = $"¡Ve al {colorName}!";
            safeColorText.color = displayColor;
        }
    }

    #endregion

    #region Game Over / Victory

    /// <summary>
    /// Show game over screen
    /// </summary>
    public void ShowGameOver(int roundReached, int totalRounds)
    {
        Debug.Log("[UI] Mostrando pantalla de Game Over");

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);
        }

        if (gameOverText != null)
        {
            gameOverText.text = $"¡PERDISTE!\n\nLlegaste a la Ronda {roundReached}\nde {totalRounds}";
        }

        // Pausar el tiempo (opcional)
        // Time.timeScale = 0f;
    }

    /// <summary>
    /// Show victory screen
    /// </summary>
    public void ShowVictory(int totalRounds)
    {
        Debug.Log("[UI] Mostrando pantalla de Victoria");

        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }

        if (victoryText != null)
        {
            victoryText.text = $"!GANASTE!\n\n¡Sobreviviste las {totalRounds} rondas!\n\n !Eres el campeón de Albion!";
        }
    }

    /// <summary>
    /// Legacy method - shows simple win text
    /// </summary>
    public void ShowWin()
    {
        if (roundText != null)
        {
            roundText.text = "¡Ganaste!";
        }
        ShowVictory(10);
    }

    /// <summary>
    /// Legacy method - shows simple lose text
    /// </summary>
    public void ShowLose()
    {
        if (roundText != null)
        {
            roundText.text = "¡Perdiste!";
        }
        ShowGameOver(0, 10);
    }

    #endregion

    #region Button Handlers

    private void OnRestartClicked()
    {
        Debug.Log("[UI] Restart clicked");
        Time.timeScale = 1f; // Restaurar tiempo si estaba pausado

        if (gameManager != null)
        {
            gameManager.RestartGame();
        }
        else
        {
            // Fallback: recargar escena directamente
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
            );
        }
    }

    private void OnMenuClicked()
    {
        Debug.Log("[UI] Menu clicked");
        Time.timeScale = 1f;

        if (gameManager != null)
        {
            gameManager.GoToMainMenu();
        }
    }

    #endregion
}