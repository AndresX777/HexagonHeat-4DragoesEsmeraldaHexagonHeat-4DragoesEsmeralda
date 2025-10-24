using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the hexagon game flow: selecting safe colors and dropping unsafe hexagons
/// </summary>
public class HexagonGameManager : MonoBehaviour
{
    [Header("Game Settings")]
    [Tooltip("Time between color changes (rounds)")]
    [SerializeField] private float roundDuration = 5f;

    [Tooltip("Warning time before hexagons fall")]
    [SerializeField] private float warningTime = 2f;

    [Tooltip("Time to wait before starting new round (for hexagons to regenerate)")]
    [SerializeField] private float regenerationTime = 2f;

    [Header("Hexagon References")]
    [Tooltip("All hexagons in the scene")]
    [SerializeField] private List<HexagonController> allHexagons = new List<HexagonController>();

    [Header("UI References (Optional)")]
    [Tooltip("Text to display safe color")]
    [SerializeField] private UnityEngine.UI.Text safeColorText;

    // Private variables
    private HexagonColor currentSafeColor;
    private bool gameStarted = false;
    private bool isWaitingForRegeneration = false;
    private int currentRound = 0;

    #region Unity Lifecycle

    private void Start()
    {
        // Auto-find all hexagons if list is empty
        if (allHexagons.Count == 0)
        {
            FindAllHexagons();
        }

        StartGame();
    }

    #endregion

    #region Game Flow

    /// <summary>
    /// Start the game
    /// </summary>
    public void StartGame()
    {
        gameStarted = true;
        currentRound = 0;
        StartCoroutine(GameLoop());
        Debug.Log("Hexagon Heat game started!");
    }

    /// <summary>
    /// Main game loop
    /// </summary>
    private IEnumerator GameLoop()
    {
        while (gameStarted)
        {
            // Start new round
            currentRound++;
            yield return StartCoroutine(RunRound());

            // Wait for hexagons to fall
            yield return new WaitForSeconds(regenerationTime);

            // Regenerate all hexagons
            RegenerateAllHexagons();

            // Wait a bit before next round
            yield return new WaitForSeconds(1f);
        }
    }

    /// <summary>
    /// Run a single round
    /// </summary>
    private IEnumerator RunRound()
    {
        // Select random safe color
        currentSafeColor = (HexagonColor)Random.Range(0, System.Enum.GetValues(typeof(HexagonColor)).Length);

        Debug.Log($"===== ROUND {currentRound} ===== Safe color: {currentSafeColor}");

        // Update UI
        if (safeColorText != null)
        {
            safeColorText.text = $"Round {currentRound}\nSafe Color: {currentSafeColor}";
        }

        // Wait for warning time (players can see the safe color)
        yield return new WaitForSeconds(warningTime);

        // Drop unsafe hexagons
        DropUnsafeHexagons();

        // Wait for round duration
        yield return new WaitForSeconds(roundDuration);
    }

    /// <summary>
    /// Drop all hexagons that don't match the safe color
    /// </summary>
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

        Debug.Log($"Unsafe hexagons are falling! Only {currentSafeColor} is safe! ({fallingCount} hexagons falling)");
    }

    /// <summary>
    /// Regenerate all hexagons for the next round
    /// </summary>
    private void RegenerateAllHexagons()
    {
        foreach (HexagonController hex in allHexagons)
        {
            if (hex != null)
            {
                hex.Regenerate();
            }
        }

        Debug.Log($"All hexagons regenerated! Ready for Round {currentRound + 1}");
    }

    #endregion

    #region Helper Methods

    /// <summary>
    /// Find all hexagons in the scene automatically
    /// </summary>
    private void FindAllHexagons()
    {
        HexagonController[] hexagons = FindObjectsOfType<HexagonController>();
        allHexagons.AddRange(hexagons);
        Debug.Log($"Found {allHexagons.Count} hexagons in the scene");
    }

    /// <summary>
    /// Get the current safe color
    /// </summary>
    public HexagonColor GetSafeColor()
    {
        return currentSafeColor;
    }

    /// <summary>
    /// Get current round number
    /// </summary>
    public int GetCurrentRound()
    {
        return currentRound;
    }

    /// <summary>
    /// Stop the game
    /// </summary>
    public void StopGame()
    {
        gameStarted = false;
        Debug.Log("Game stopped!");
    }

    #endregion
}
