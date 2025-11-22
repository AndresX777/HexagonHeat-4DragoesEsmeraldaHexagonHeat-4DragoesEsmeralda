using UnityEngine;
using TMPro;

public class UIManager : MonoBehaviour
{
    public TextMeshProUGUI roundText;

    public void UpdateRound(int roundNumber)
    {
        roundText.text = "Ronda " + roundNumber;
    }

    public void ShowWin()
    {
        roundText.text = "¡Ganaste!";
    }

    public void ShowLose()
    {
        roundText.text = "¡Perdiste!";
    }
}
