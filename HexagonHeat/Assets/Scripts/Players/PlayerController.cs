using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Controls manual player movement using WASD input.
/// Handles hexagon selection and movement validation.
/// </summary>
public class PlayerController : MonoBehaviour
{
    // ============ REFERENCES ============
    private Player player;
    private HexagonGameManager hexagonGameManager;

    // ============ INPUT ============
    private float inputCooldown = 0.2f;
    private float inputTimer = 0f;

    // ============ HEXAGON NAVIGATION ============
    private int currentHexagonId;
    private List<int> adjacentHexagons = new List<int>();

    #region INITIALIZATION

    /// <summary>
    /// Initialize the player controller
    /// </summary>
    public void Initialize(Player targetPlayer, HexagonGameManager gameManager)
    {
        player = targetPlayer;
        hexagonGameManager = gameManager;
        currentHexagonId = player.currentHexagonId;

        Debug.Log("[PLAYER CONTROLLER] Manual player initialized - WASD Controls Ready");
    }

    #endregion

    #region UPDATE

    /// <summary>
    /// Called every frame to handle input
    /// </summary>
    public void HandleInput()
    {
        if (player == null || !player.IsAlive())
            return;

        // Cooldown between moves
        if (inputTimer > 0)
        {
            inputTimer -= Time.deltaTime;
            return;
        }

        // Check input
        bool movedW = Input.GetKeyDown(KeyCode.W);
        bool movedA = Input.GetKeyDown(KeyCode.A);
        bool movedS = Input.GetKeyDown(KeyCode.S);
        bool movedD = Input.GetKeyDown(KeyCode.D);

        if (movedW || movedA || movedS || movedD)
        {
            int targetHexagon = GetTargetHexagon(movedW, movedA, movedS, movedD);

            if (targetHexagon != -1)
            {
                MoveToHexagon(targetHexagon);
                inputTimer = inputCooldown; // Reset cooldown
            }
        }
    }

    #endregion

    #region MOVEMENT

    /// <summary>
    /// Determine target hexagon based on input direction
    /// </summary>
    private int GetTargetHexagon(bool up, bool left, bool down, bool right)
    {
        GetAdjacentHexagons();

        // Simple directional movement
        // In a real hex grid, directions would be more complex
        // For now, we use a simplified version

        if (up && adjacentHexagons.Count > 0)
            return adjacentHexagons[0];

        if (left && adjacentHexagons.Count > 1)
            return adjacentHexagons[1];

        if (down && adjacentHexagons.Count > 2)
            return adjacentHexagons[2];

        if (right && adjacentHexagons.Count > 3)
            return adjacentHexagons[3];

        return -1;
    }

    /// <summary>
    /// Move player to target hexagon
    /// </summary>
    private void MoveToHexagon(int targetHexId)
    {
        if (hexagonGameManager == null)
        {
            Debug.LogWarning("[PLAYER CONTROLLER] HexagonGameManager not assigned!");
            return;
        }

        // Get target position
        Vector3 targetPosition = hexagonGameManager.GetHexagonPosition(targetHexId);

        // Move player
        player.MoveToHexagon(targetHexId, targetPosition);
        currentHexagonId = targetHexId;

        Debug.Log($"[PLAYER CONTROLLER] {player.playerName} moved to hexagon {targetHexId}");
    }

    /// <summary>
    /// Get adjacent hexagons from current position
    /// </summary>
    private void GetAdjacentHexagons()
    {
        adjacentHexagons.Clear();

        if (hexagonGameManager != null)
        {
            adjacentHexagons = hexagonGameManager.GetAdjacentHexagons(currentHexagonId);
        }
    }

    #endregion

    #region DEBUG

    private void OnGUI()
    {
        if (player == null || !player.IsAlive())
            return;

        // Display controls
        GUI.Label(new Rect(10, 10, 300, 100),
            $"Player: {player.playerName}\n" +
            $"Position: Hex {currentHexagonId}\n" +
            $"Controls: WASD to move");
    }

    #endregion
}