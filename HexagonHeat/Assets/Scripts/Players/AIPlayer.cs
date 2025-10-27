using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// AI-controlled player that attempts to stay on safe hexagons.
/// Implements difficult AI that prioritizes survival.
/// </summary>
public class AIPlayer : MonoBehaviour
{
    // ============ REFERENCES ============
    private Player player;
    private HexagonGameManager hexagonGameManager;

    // ============ AI BEHAVIOR ============
    private float movementInterval = 1.5f;  // Time between AI decisions
    private float movementTimer = 0f;
    private int currentHexagonId;

    // ============ DIFFICULTY SETTINGS ============
    [SerializeField] private float safetyPriority = 0.9f; // 0-1: How much AI prioritizes safety

    #region INITIALIZATION

    /// <summary>
    /// Initialize the AI player
    /// </summary>
    public void Initialize(Player targetPlayer, HexagonGameManager gameManager)
    {
        player = targetPlayer;
        hexagonGameManager = gameManager;
        currentHexagonId = player.currentHexagonId;
        movementTimer = Random.Range(0.5f, 1.5f); // Stagger AI moves

        Debug.Log($"[AI PLAYER] {player.playerName} initialized with AI control");
    }

    #endregion

    #region UPDATE

    /// <summary>
    /// Called every frame to make AI decisions
    /// </summary>
    public void AIUpdate()
    {
        if (player == null || !player.IsAlive())
            return;

        movementTimer -= Time.deltaTime;

        if (movementTimer <= 0)
        {
            CalculateNextMove();
            movementTimer = movementInterval;
        }
    }

    #endregion

    #region AI DECISION MAKING

    /// <summary>
    /// Calculate and execute next movement
    /// </summary>
    private void CalculateNextMove()
    {
        // Get adjacent hexagons
        List<int> adjacentHexagons = hexagonGameManager.GetAdjacentHexagons(currentHexagonId);

        if (adjacentHexagons == null || adjacentHexagons.Count == 0)
        {
            Debug.LogWarning($"[AI PLAYER] No adjacent hexagons for {player.playerName}!");
            return;
        }

        // Get safe hexagons (DIFFICULT: Try to avoid falls)
        List<int> safeHexagons = GetSafeAdjacentHexagons(adjacentHexagons);

        int targetHexagon;

        if (safeHexagons.Count > 0)
        {
            // DIFFICULT MODE: Prioritize safe hexagons
            targetHexagon = ChooseBestHexagon(safeHexagons);
            Debug.Log($"[AI PLAYER] {player.playerName} chooses safe hexagon: {targetHexagon}");
        }
        else
        {
            // No safe option available, pick randomly
            targetHexagon = adjacentHexagons[Random.Range(0, adjacentHexagons.Count)];
            Debug.LogWarning($"[AI PLAYER] {player.playerName} forced to take risky move: {targetHexagon}");
        }

        // Move to target
        MoveToHexagon(targetHexagon);
    }

    /// <summary>
    /// Filter adjacent hexagons to only safe ones
    /// </summary>
    private List<int> GetSafeAdjacentHexagons(List<int> adjacentHexagons)
    {
        List<int> safeHexagons = new List<int>();
        List<int> eliminatedHexagons = hexagonGameManager.GetEliminatedHexagons();

        foreach (int hexId in adjacentHexagons)
        {
            if (!eliminatedHexagons.Contains(hexId))
            {
                safeHexagons.Add(hexId);
            }
        }

        return safeHexagons;
    }

    /// <summary>
    /// Choose the best hexagon from available options
    /// Considers distance from edges, safety, etc.
    /// </summary>
    private int ChooseBestHexagon(List<int> safeOptions)
    {
        if (safeOptions.Count == 0)
            return -1;

        if (safeOptions.Count == 1)
            return safeOptions[0];

        // DIFFICULT: Choose hexagon furthest from eliminated areas
        int bestHexagon = safeOptions[0];
        float bestScore = float.MinValue;

        foreach (int hexId in safeOptions)
        {
            float score = CalculateHexagonScore(hexId);

            if (score > bestScore)
            {
                bestScore = score;
                bestHexagon = hexId;
            }
        }

        return bestHexagon;
    }

    /// <summary>
    /// Calculate a safety score for a hexagon
    /// Higher score = better choice
    /// </summary>
    private float CalculateHexagonScore(int hexagonId)
    {
        float score = 0f;

        // Base score (prefer staying put)
        if (hexagonId == currentHexagonId)
            score += 1f;

        // Distance from center (prefer center hexagons)
        float distanceFromCenter = hexagonGameManager.GetHexagonDistanceFromCenter(hexagonId);
        score += (1f / (distanceFromCenter + 1f)); // Inverse distance = closer = higher score

        // Add small randomness to avoid predictable patterns
        score += Random.Range(-0.1f, 0.1f);

        return score;
    }

    #endregion

    #region MOVEMENT

    /// <summary>
    /// Move AI player to target hexagon
    /// </summary>
    private void MoveToHexagon(int targetHexId)
    {
        if (hexagonGameManager == null)
        {
            Debug.LogWarning($"[AI PLAYER] HexagonGameManager not assigned for {player.playerName}!");
            return;
        }

        // Get target position
        Vector3 targetPosition = hexagonGameManager.GetHexagonPosition(targetHexId);

        // Move player
        player.MoveToHexagon(targetHexId, targetPosition);
        currentHexagonId = targetHexId;

        Debug.Log($"[AI PLAYER] {player.playerName} moved to hexagon {targetHexId}");
    }

    #endregion

    #region DEBUG

    /// <summary>
    /// Get AI difficulty level (for UI display)
    /// </summary>
    public string GetDifficultyLevel()
    {
        return "HARD";
    }

    #endregion
}