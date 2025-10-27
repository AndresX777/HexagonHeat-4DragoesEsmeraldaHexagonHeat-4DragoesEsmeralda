using UnityEngine;

/// <summary>
/// Represents a single player in the Hexagon game.
/// Stores player data, state, and references.
/// FIXED: playerVisual is now public so PlayerManager can assign it
/// </summary>
public class Player : MonoBehaviour
{
    // ============ PLAYER IDENTIFICATION ============
    public int playerId;                    // 0 = Manual Player, 1-3 = AI Players
    public string playerName;               // "Vampire", "Eva", "Kachujin", "Mutant"
    public PlayerColor playerColor;         // Color assigned to this player

    // ============ PLAYER STATE ============
    public PlayerState currentState;        // Alive, Falling, Dead
    public int currentHexagonId;            // ID of hexagon player is on
    public Vector3 worldPosition;           // 3D position in the world

    // ============ PLAYER PROPERTIES ============
    public bool isManualControl;            // true = WASD control, false = AI control
    public float moveSpeed = 5f;            // Units per second
    public int livesRemaining = 1;          // Lives left (1 = dies immediately)

    // ============ VISUALIZATION ============
    public GameObject playerVisual;         // Visual representation (prefab instance) - CHANGED TO PUBLIC
    private Material playerMaterial;        // Material with player color

    // ============ INTERNAL STATE ============
    private bool isMoving = false;
    private Vector3 targetPosition;
    private float moveProgress = 0f;

    #region PUBLIC METHODS

    /// <summary>
    /// Initialize player with all necessary data
    /// </summary>
    public void Initialize(int id, string name, PlayerColor color, bool isManual)
    {
        playerId = id;
        playerName = name;
        playerColor = color;
        isManualControl = isManual;
        currentState = PlayerState.Alive;
        livesRemaining = 1;

        Debug.Log($"[PLAYER] Initialized: {playerName} (ID: {playerId}, Manual: {isManual})");
    }

    /// <summary>
    /// Move player to a specific hexagon
    /// </summary>
    public void MoveToHexagon(int hexagonId, Vector3 targetPos)
    {
        if (currentState != PlayerState.Alive)
            return;

        currentHexagonId = hexagonId;
        targetPosition = targetPos;
        isMoving = true;
        moveProgress = 0f;
    }

    /// <summary>
    /// Called every frame to update player position
    /// </summary>
    public void UpdatePosition()
    {
        if (!isMoving || currentState != PlayerState.Alive)
            return;

        moveProgress += Time.deltaTime * moveSpeed;

        // Smooth lerp between current and target position
        worldPosition = Vector3.Lerp(worldPosition, targetPosition, moveProgress);

        // Stop moving when reached target
        if (moveProgress >= 1f)
        {
            worldPosition = targetPosition;
            isMoving = false;
            moveProgress = 0f;
        }

        // Update visual position
        if (playerVisual != null)
            playerVisual.transform.position = worldPosition;
    }

    /// <summary>
    /// Kill this player (eliminate from game)
    /// </summary>
    public void Die()
    {
        currentState = PlayerState.Dead;
        livesRemaining = 0;

        Debug.Log($"[PLAYER] {playerName} has been eliminated!");

        // Destroy visual representation
        if (playerVisual != null)
            Destroy(playerVisual);
    }

    /// <summary>
    /// Check if player is alive
    /// </summary>
    public bool IsAlive()
    {
        return currentState == PlayerState.Alive;
    }

    /// <summary>
    /// Set player as winner
    /// </summary>
    public void SetAsWinner()
    {
        currentState = PlayerState.Winner;
        Debug.Log($"[PLAYER] {playerName} is the WINNER!");
    }

    /// <summary>
    /// Get player color as Color type
    /// </summary>
    public Color GetPlayerColorValue()
    {
        return playerColor switch
        {
            PlayerColor.Red => Color.red,
            PlayerColor.Blue => Color.blue,
            PlayerColor.Green => Color.green,
            PlayerColor.Yellow => Color.yellow,
            _ => Color.white
        };
    }

    #endregion

    #region INTERNAL METHODS

    private void OnDestroy()
    {
        if (playerVisual != null)
            Destroy(playerVisual);
    }

    #endregion
}

/// <summary>
/// Enum for player colors
/// </summary>
public enum PlayerColor
{
    Red,        // Vampire
    Blue,       // Eva
    Green,      // Kachujin
    Yellow      // Mutant
}

/// <summary>
/// Enum for player states
/// </summary>
public enum PlayerState
{
    Alive,
    Falling,
    Dead,
    Winner
}