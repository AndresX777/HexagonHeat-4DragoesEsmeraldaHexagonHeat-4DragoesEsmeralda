using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages the hexagon game flow: selecting safe colors and dropping unsafe hexagons
/// MODIFIED: Added player system integration
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

    [Header("Player System References")]
    [SerializeField] private PlayerManager playerManager;
    [SerializeField] private PlayerUIManager playerUIManager;

    [Header("UI References (Optional)")]
    [Tooltip("Text to display safe color")]
    [SerializeField] private UnityEngine.UI.Text safeColorText;

    [Header("Visual Elements")]
    [Tooltip("Bandera controller for Albion")]
    [SerializeField] private BanderaController banderaController;

    // Private variables
    private HexagonColor currentSafeColor;
    private bool gameStarted = false;
    private bool isWaitingForRegeneration = false;
    private int currentRound = 0;
    private List<int> eliminatedHexagons = new List<int>();
    private List<int> safeHexagons = new List<int>();

    #region Unity Lifecycle

    private void Start()
    {
        // Auto-find all hexagons if list is empty
        if (allHexagons.Count == 0)
        {
            FindAllHexagons();
        }

        // Initialize players
        if (playerManager != null)
        {
            playerManager.InitializePlayers(4); // 4 players (1 manual + 3 AI)
        }

        if (playerUIManager != null && playerManager != null)
        {
            playerUIManager.ShowPlayerList(playerManager.GetAlivePlayers());
        }

        StartGame();
    }

    private void Update()
    {
        // Update all players
        if (playerManager != null)
        {
            // Update player positions
            playerManager.UpdateAllPlayers();

            // Handle manual player input (WASD)
            Player manualPlayer = playerManager.GetManualPlayer();
            if (manualPlayer != null && manualPlayer.IsAlive())
            {
                PlayerController controller = manualPlayer.GetComponent<PlayerController>();
                if (controller != null)
                    controller.HandleInput();
            }

            // Update AI players
            foreach (Player player in playerManager.GetAlivePlayers())
            {
                if (!player.isManualControl)
                {
                    AIPlayer aiController = player.GetComponent<AIPlayer>();
                    if (aiController != null)
                        aiController.AIUpdate();
                }
            }
        }

        // Update UI
        if (playerUIManager != null)
        {
            playerUIManager.UpdateAllPlayerStatus();
        }
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
        Debug.Log("[GAME] Hexagon Heat game started!");
    }

    /// <summary>
    /// Main game loop
    /// </summary>
    private IEnumerator GameLoop()
    {
        while (gameStarted)
        {
            // Check if game should end (winner found)
            if (playerManager != null && playerManager.GameEnded)
            {
                Debug.Log("[GAME] Game ended - Winner found!");
                gameStarted = false;
                break;
            }

            // Start new round
            currentRound++;
            yield return StartCoroutine(RunRound());

            // Wait for hexagons to fall
            yield return new WaitForSeconds(regenerationTime);

            // Regenerate all hexagons
            RegenerateAllHexagons();

            // Reset for next round
            if (playerManager != null)
                playerManager.ResetForNextRound();

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

        // Calculate safe hexagons based on color
        CalculateSafeHexagons();

        // Update UI
        if (safeColorText != null)
        {
            safeColorText.text = $"Round {currentRound}\nSafe Color: {currentSafeColor}";
        }

        // Show flag BEFORE hexagons fall
        if (banderaController != null)
        {
            banderaController.ShowFlag(currentSafeColor);
            Debug.Log($"[GAME] Albion showing {currentSafeColor} flag!");
        }

        // Wait for warning time (players see the flag and safe color)
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
        eliminatedHexagons.Clear();

        foreach (HexagonController hex in allHexagons)
        {
            if (hex != null)
            {
                bool isSafe = (hex.GetColor() == currentSafeColor);
                hex.SetSafeState(isSafe);

                if (!isSafe)
                {
                    fallingCount++;
                    // Track which hexagons are eliminated
                    int hexId = allHexagons.IndexOf(hex);
                    eliminatedHexagons.Add(hexId);
                }
            }
        }

        // Notify player manager about eliminated hexagons
        OnHexagonsEliminated(eliminatedHexagons);

        Debug.Log($"[GAME] Unsafe hexagons are falling! Only {currentSafeColor} is safe! ({fallingCount} hexagons falling)");
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

        Debug.Log($"[GAME] All hexagons regenerated! Ready for Round {currentRound + 1}");
    }

    #endregion

    #region Player System Integration

    /// <summary>
    /// Called when hexagons are eliminated
    /// Notifies PlayerManager to check for deaths
    /// </summary>
    public void OnHexagonsEliminated(List<int> eliminatedIds)
    {
        eliminatedHexagons = new List<int>(eliminatedIds);

        // Notify player manager to check for deaths
        if (playerManager != null)
        {
            playerManager.UpdateAllPlayers();
        }
    }

    /// <summary>
    /// Get hexagon world position by ID (index in list)
    /// </summary>
    public Vector3 GetHexagonPosition(int hexagonId)
    {
        if (hexagonId >= 0 && hexagonId < allHexagons.Count && allHexagons[hexagonId] != null)
        {
            return allHexagons[hexagonId].transform.position;
        }

        Debug.LogWarning($"[GAME] Hexagon ID {hexagonId} not found!");
        return Vector3.zero;
    }

    /// <summary>
    /// Get adjacent hexagons for a given hexagon
    /// In a circular arrangement, neighbors are index +1 and -1
    /// </summary>
    public List<int> GetAdjacentHexagons(int hexagonId)
    {
        List<int> adjacent = new List<int>();
        int totalHexagons = GetTotalHexagonCount();

        if (totalHexagons == 0)
            return adjacent;

        // Left neighbor
        int left = (hexagonId - 1 + totalHexagons) % totalHexagons;
        adjacent.Add(left);

        // Right neighbor
        int right = (hexagonId + 1) % totalHexagons;
        adjacent.Add(right);

        // TODO: Add more neighbors if your grid is not circular
        // For example, if hexagons are arranged in rows/columns:
        // adjacent.Add(hexagonId - rowWidth);  // Top
        // adjacent.Add(hexagonId + rowWidth);  // Bottom

        return adjacent;
    }

    /// <summary>
    /// Get hexagons that are eliminated this round
    /// </summary>
    public List<int> GetEliminatedHexagons()
    {
        return new List<int>(eliminatedHexagons);
    }

    /// <summary>
    /// Get safe hexagons for this round
    /// </summary>
    public List<int> GetSafeHexagons()
    {
        return new List<int>(safeHexagons);
    }

    /// <summary>
    /// Calculate which hexagons are safe this round
    /// </summary>
    private void CalculateSafeHexagons()
    {
        safeHexagons.Clear();

        for (int i = 0; i < allHexagons.Count; i++)
        {
            if (allHexagons[i] != null && allHexagons[i].GetColor() == currentSafeColor)
            {
                safeHexagons.Add(i);
            }
        }
    }

    /// <summary>
    /// Get total number of hexagons
    /// </summary>
    public int GetTotalHexagonCount()
    {
        return allHexagons != null ? allHexagons.Count : 0;
    }

    /// <summary>
    /// Get distance of hexagon from center
    /// Assumes hexagon at index 0 is center
    /// </summary>
    public float GetHexagonDistanceFromCenter(int hexagonId)
    {
        Vector3 hexPos = GetHexagonPosition(hexagonId);
        Vector3 centerPos = GetHexagonPosition(0);

        return Vector3.Distance(hexPos, centerPos);
    }

    /// <summary>
    /// Check if a player is on a specific hexagon
    /// </summary>
    public bool IsPlayerOnHexagon(int hexagonId, Player player)
    {
        return player.currentHexagonId == hexagonId;
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
        Debug.Log($"[GAME] Found {allHexagons.Count} hexagons in the scene");
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
        Debug.Log("[GAME] Game stopped!");
    }

    #endregion
}