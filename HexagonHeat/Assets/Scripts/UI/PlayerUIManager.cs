using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Manages all player-related UI elements.
/// Displays player status, lives, eliminations, and winner.
/// </summary>
public class PlayerUIManager : MonoBehaviour
{
    // ============ UI REFERENCES ============
    [SerializeField] private Canvas mainCanvas;
    [SerializeField] private Transform playerStatusPanel;
    [SerializeField] private TextMeshProUGUI winnerText;
    [SerializeField] private TextMeshProUGUI roundInfoText;

    // ============ PREFABS ============
    [SerializeField] private GameObject playerStatusPrefab;

    // ============ UI CONTAINERS ============
    private List<PlayerStatusUI> playerStatusUIList = new List<PlayerStatusUI>();
    private Dictionary<Player, PlayerStatusUI> playerToStatusUI = new Dictionary<Player, PlayerStatusUI>();

    #region INITIALIZATION

    private void Start()
    {
        if (mainCanvas == null)
            mainCanvas = FindObjectOfType<Canvas>();

        // IMPORTANTE: Desactiva el texto del ganador al inicio
        if (winnerText != null)
        {
            winnerText.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Initialize UI for all players
    /// </summary>
    public void ShowPlayerList(List<Player> players)
    {
        ClearPlayerUI();

        foreach (Player player in players)
        {
            AddPlayerStatus(player);
        }

        Debug.Log("[PLAYER UI] Displayed status for " + players.Count + " players");
    }

    /// <summary>
    /// Add a player status UI element
    /// </summary>
    private void AddPlayerStatus(Player player)
    {
        GameObject statusObj = new GameObject($"PlayerStatus_{player.playerName}");
        statusObj.transform.SetParent(playerStatusPanel, false);

        TextMeshProUGUI statusText = statusObj.AddComponent<TextMeshProUGUI>();
        statusText.text = $"{player.playerName}: ALIVE";
        statusText.color = player.GetPlayerColorValue();
        statusText.fontSize = 36;

        PlayerStatusUI statusUI = new PlayerStatusUI
        {
            player = player,
            statusText = statusText
        };

        playerStatusUIList.Add(statusUI);
        playerToStatusUI[player] = statusUI;
    }

    /// <summary>
    /// Clear all player UI elements
    /// </summary>
    private void ClearPlayerUI()
    {
        foreach (Transform child in playerStatusPanel)
        {
            Destroy(child.gameObject);
        }

        playerStatusUIList.Clear();
        playerToStatusUI.Clear();
    }

    #endregion

    #region UPDATE METHODS

    /// <summary>
    /// Update all player status displays
    /// </summary>
    public void UpdateAllPlayerStatus()
    {
        foreach (PlayerStatusUI statusUI in playerStatusUIList)
        {
            if (statusUI.player == null)
                continue;

            string state = statusUI.player.currentState.ToString();
            statusUI.statusText.text = $"{statusUI.player.playerName}: {state}";

            // Change color based on state
            if (statusUI.player.currentState == PlayerState.Dead)
                statusUI.statusText.color = Color.gray;
            else
                statusUI.statusText.color = statusUI.player.GetPlayerColorValue();
        }
    }

    /// <summary>
    /// Show when a player is eliminated
    /// </summary>
    public void ShowPlayerEliminated(Player player)
    {
        if (!playerToStatusUI.ContainsKey(player))
            return;

        PlayerStatusUI statusUI = playerToStatusUI[player];
        statusUI.statusText.text = $"{player.playerName}: ELIMINATED";
        statusUI.statusText.color = Color.gray;

        // Optional: Play elimination animation
        StartCoroutine(EliminationFlash(statusUI.statusText));
    }

    /// <summary>
    /// Show round information
    /// </summary>
    public void ShowRoundInfo(int roundNumber, int alivePlayers)
    {
        if (roundInfoText != null)
        {
            roundInfoText.text = $"Round {roundNumber} | Players Alive: {alivePlayers}";
        }
    }

    #endregion

    #region WINNER DISPLAY

    /// <summary>
    /// Display the winner - SOLO SE ACTIVA CUANDO HAY GANADOR
    /// </summary>
    public void ShowWinner(Player winner)
    {
        if (winnerText == null)
        {
            Debug.LogWarning("[PLAYER UI] Winner text not assigned!");
            return;
        }

        // ACTIVAR SOLO CUANDO GANA ALGUIEN
        winnerText.gameObject.SetActive(true);

        winnerText.text = $"🎉 WINNER: {winner.playerName.ToUpper()} 🎉";
        winnerText.color = winner.GetPlayerColorValue();
        winnerText.fontSize = 60;

        Debug.Log($"[PLAYER UI] Displayed winner: {winner.playerName}");

        StartCoroutine(WinnerAnimation(winnerText));
    }

    /// <summary>
    /// Ocultar el texto del ganador
    /// </summary>
    public void HideWinner()
    {
        if (winnerText != null)
        {
            winnerText.gameObject.SetActive(false);
        }
    }

    #endregion

    #region ANIMATIONS

    /// <summary>
    /// Flash animation for eliminated players
    /// </summary>
    private System.Collections.IEnumerator EliminationFlash(TextMeshProUGUI text)
    {
        for (int i = 0; i < 3; i++)
        {
            text.alpha = 0.5f;
            yield return new WaitForSeconds(0.2f);
            text.alpha = 1f;
            yield return new WaitForSeconds(0.2f);
        }
    }

    /// <summary>
    /// Animation for winner display
    /// </summary>
    private System.Collections.IEnumerator WinnerAnimation(TextMeshProUGUI text)
    {
        for (int i = 0; i < 5; i++)
        {
            text.fontSize = 60 + (i * 5);
            yield return new WaitForSeconds(0.1f);
        }
    }

    #endregion

    #region DEBUG

    [ContextMenu("Debug: Show Test Winner")]
    public void DebugShowWinner()
    {
        if (playerStatusUIList.Count > 0)
        {
            ShowWinner(playerStatusUIList[0].player);
        }
    }

    [ContextMenu("Debug: Hide Test Winner")]
    public void DebugHideWinner()
    {
        HideWinner();
    }

    #endregion
}

/// <summary>
/// Helper class to store player UI references
/// </summary>
[System.Serializable]
public class PlayerStatusUI
{
    public Player player;
    public TextMeshProUGUI statusText;
}