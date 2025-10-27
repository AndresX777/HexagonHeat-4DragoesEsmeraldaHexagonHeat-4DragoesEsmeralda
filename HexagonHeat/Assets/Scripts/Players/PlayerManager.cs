using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Manages all players in the game.
/// Handles initialization, updates, elimination, and win conditions.
/// IMPROVED: Now uses real character models instead of spheres
/// </summary>
public class PlayerManager : MonoBehaviour
{
    // ============ REFERENCES ============
    public HexagonGameManager hexagonGameManager;
    public PlayerUIManager playerUIManager;

    // ============ PLAYER DATA ============
    [SerializeField] private int numberOfPlayers = 1;
    private List<Player> activePlayers = new List<Player>();
    private List<Player> allPlayers = new List<Player>();
    private Player manualPlayer;
    private Player currentWinner;

    // ============ CHARACTER PREFABS ============
    private Dictionary<string, GameObject> characterPrefabs = new Dictionary<string, GameObject>();

    // ============ GAME STATE ============
    public int AlivePlayers { get => activePlayers.Count; }
    public bool GameEnded { get; private set; } = false;

    #region INITIALIZATION

    private void Start()
    {
        // Load character prefabs from Assets/Models/
        LoadCharacterPrefabs();
    }

    /// <summary>
    /// Load all character prefabs from the Models folder
    /// </summary>
    private void LoadCharacterPrefabs()
    {
        characterPrefabs["Vampire"] = Resources.Load<GameObject>("Models/character_vampire a lusth/character_vampire a lusth");
        characterPrefabs["Eva"] = Resources.Load<GameObject>("Models/character_Eva/character_eva");
        characterPrefabs["Kachujin"] = Resources.Load<GameObject>("Models/character_kachujin G Rosales/character_kachujin G Rosales");
        characterPrefabs["Mutant"] = Resources.Load<GameObject>("Models/character_mutante/character_mutante");

        // Log loaded prefabs
        foreach (var kvp in characterPrefabs)
        {
            if (kvp.Value == null)
                Debug.LogWarning($"[PLAYER MANAGER] Failed to load prefab for {kvp.Key}");
            else
                Debug.Log($"[PLAYER MANAGER] Loaded prefab: {kvp.Key}");
        }
    }

    /// <summary>
    /// Initialize all players for the game
    /// </summary>
    public void InitializePlayers(int playerCount)
    {
        if (playerCount < 1 || playerCount > 4)
        {
            Debug.LogError("[PLAYER MANAGER] Invalid player count! Must be 1-4");
            playerCount = Mathf.Clamp(playerCount, 1, 4);
        }

        numberOfPlayers = playerCount;
        activePlayers.Clear();
        allPlayers.Clear();
        GameEnded = false;

        // Player data
        string[] playerNames = { "Vampire", "Eva", "Kachujin", "Mutant" };
        PlayerColor[] playerColors = { PlayerColor.Red, PlayerColor.Blue, PlayerColor.Green, PlayerColor.Yellow };

        // Create players
        for (int i = 0; i < playerCount; i++)
        {
            Player newPlayer = CreatePlayer(i, playerNames[i], playerColors[i], i == 0);
            activePlayers.Add(newPlayer);
            allPlayers.Add(newPlayer);

            if (i == 0)
                manualPlayer = newPlayer;
        }

        // Update UI
        if (playerUIManager != null)
            playerUIManager.ShowPlayerList(activePlayers);

        Debug.Log($"[PLAYER MANAGER] Initialized {playerCount} players");
    }

    /// <summary>
    /// Create a single player with real character model
    /// IMPROVED: Uses character prefabs instead of spheres
    /// </summary>
    private Player CreatePlayer(int playerId, string name, PlayerColor color, bool isManual)
    {
        GameObject playerObj = new GameObject($"Player_{name}");
        playerObj.transform.SetParent(transform);

        Player player = playerObj.AddComponent<Player>();
        player.Initialize(playerId, name, color, isManual);

        // ============ INSTANTIATE CHARACTER MODEL ============
        GameObject characterModel = null;

        if (characterPrefabs.ContainsKey(name) && characterPrefabs[name] != null)
        {
            characterModel = Instantiate(characterPrefabs[name]);
            characterModel.name = $"Visual_{name}";
            characterModel.transform.SetParent(playerObj.transform);
            characterModel.transform.localPosition = Vector3.zero;
            characterModel.transform.localScale = Vector3.one;

            // Get Animator if it exists
            Animator animator = characterModel.GetComponent<Animator>();
            if (animator == null)
            {
                animator = characterModel.AddComponent<Animator>();
                Debug.Log($"[PLAYER MANAGER] Added Animator component to {name}");
            }

            Debug.Log($"[PLAYER MANAGER] Instantiated character model for {name}");
        }
        else
        {
            Debug.LogWarning($"[PLAYER MANAGER] Character prefab not found for {name}, creating fallback sphere");

            // FALLBACK: Create sphere if model not found
            characterModel = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            characterModel.name = $"Visual_{name}";
            characterModel.transform.SetParent(playerObj.transform);
            characterModel.transform.localPosition = Vector3.zero;
            characterModel.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);

            // Apply player color
            Renderer renderer = characterModel.GetComponent<Renderer>();
            Material mat = new Material(Shader.Find("Standard"));
            mat.color = player.GetPlayerColorValue();
            renderer.material = mat;

            // Remove collider
            Collider collider = characterModel.GetComponent<Collider>();
            if (collider != null)
                DestroyImmediate(collider);
        }

        // ============ ASSIGN VISUAL TO PLAYER ============
        player.playerVisual = characterModel;

        // ============ END CHARACTER MODEL ============

        // Add controller
        if (isManual)
        {
            PlayerController controller = playerObj.AddComponent<PlayerController>();
            controller.Initialize(player, hexagonGameManager);
        }
        else
        {
            AIPlayer aiController = playerObj.AddComponent<AIPlayer>();
            aiController.Initialize(player, hexagonGameManager);
        }

        // Assign starting hexagon (center)
        int startingHexId = 0; // Center hexagon
        Vector3 startingPos = GetHexagonWorldPosition(startingHexId);
        player.currentHexagonId = startingHexId;
        player.worldPosition = startingPos;

        Debug.Log($"[PLAYER MANAGER] Created Player: {name} (ID: {playerId}, Manual: {isManual})");

        return player;
    }

    #endregion

    #region UPDATE METHODS

    /// <summary>
    /// Update all players every frame
    /// </summary>
    public void UpdateAllPlayers()
    {
        foreach (Player player in activePlayers)
        {
            if (player.IsAlive())
                player.UpdatePosition();
        }

        // Check for eliminated players
        CheckForEliminatedPlayers();

        // Check win condition
        CheckWinCondition();
    }

    #endregion

    #region PLAYER ELIMINATION

    /// <summary>
    /// Check if any players are in eliminated hexagons
    /// </summary>
    private void CheckForEliminatedPlayers()
    {
        if (hexagonGameManager == null)
            return;

        List<int> eliminatedHexagons = hexagonGameManager.GetEliminatedHexagons();

        // Create a copy to avoid conflicts while iterating
        List<Player> playersToRemove = new List<Player>();

        foreach (Player player in activePlayers)
        {
            if (!player.IsAlive())
                continue;

            // Mark players for elimination
            if (eliminatedHexagons.Contains(player.currentHexagonId))
            {
                playersToRemove.Add(player);
            }
        }

        // Remove after iterating
        foreach (Player player in playersToRemove)
        {
            EliminatePlayer(player);
        }
    }

    /// <summary>
    /// Eliminate a player from the game
    /// </summary>
    public void EliminatePlayer(Player player)
    {
        if (!player.IsAlive())
            return;

        player.Die();
        activePlayers.Remove(player);

        // Update UI
        if (playerUIManager != null)
            playerUIManager.ShowPlayerEliminated(player);

        Debug.Log($"[PLAYER MANAGER] {player.playerName} has been eliminated! ({activePlayers.Count} alive)");
    }

    #endregion

    #region WIN CONDITION

    /// <summary>
    /// Check if there is a winner
    /// </summary>
    private void CheckWinCondition()
    {
        if (GameEnded)
            return;

        // If only 1 player left, they win
        if (activePlayers.Count == 1)
        {
            currentWinner = activePlayers[0];
            currentWinner.SetAsWinner();
            GameEnded = true;

            if (playerUIManager != null)
                playerUIManager.ShowWinner(currentWinner);

            Debug.Log($"[PLAYER MANAGER] GAME ENDED! Winner: {currentWinner.playerName}");
        }

        // If all players dead
        if (activePlayers.Count == 0)
        {
            GameEnded = true;
            Debug.LogWarning("[PLAYER MANAGER] All players eliminated!");
        }
    }

    /// <summary>
    /// Get the current winner
    /// </summary>
    public Player GetWinner()
    {
        return currentWinner;
    }

    #endregion

    #region UTILITY METHODS

    /// <summary>
    /// Get a player by ID
    /// </summary>
    public Player GetPlayerById(int playerId)
    {
        return allPlayers.Find(p => p.playerId == playerId);
    }

    /// <summary>
    /// Get all alive players
    /// </summary>
    public List<Player> GetAlivePlayers()
    {
        return new List<Player>(activePlayers);
    }

    /// <summary>
    /// Get the manual player (Player 1)
    /// </summary>
    public Player GetManualPlayer()
    {
        return manualPlayer;
    }

    /// <summary>
    /// Get world position of a hexagon
    /// </summary>
    private Vector3 GetHexagonWorldPosition(int hexagonId)
    {
        if (hexagonGameManager != null)
            return hexagonGameManager.GetHexagonPosition(hexagonId);

        return Vector3.zero;
    }

    /// <summary>
    /// Reset for next round
    /// </summary>
    public void ResetForNextRound()
    {
        activePlayers.Clear();
        foreach (Player player in allPlayers)
        {
            if (player.IsAlive() || player.currentState == PlayerState.Falling)
            {
                activePlayers.Add(player);
                player.currentState = PlayerState.Alive;
            }
        }

        GameEnded = false;
        currentWinner = null;
    }

    #endregion

    #region DEBUG

    [ContextMenu("Debug: Print All Players")]
    public void DebugPrintPlayers()
    {
        Debug.Log("=== ALL PLAYERS ===");
        foreach (Player player in allPlayers)
        {
            Debug.Log($"{player.playerName}: {player.currentState} (Hex: {player.currentHexagonId})");
        }
    }

    #endregion
}