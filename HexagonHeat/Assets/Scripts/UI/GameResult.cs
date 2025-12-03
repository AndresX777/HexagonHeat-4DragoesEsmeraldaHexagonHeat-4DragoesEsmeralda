using UnityEngine;

public class GameResult : MonoBehaviour
{
    public static GameResult Instance;
    public bool playerWon = false;
    public bool gameEnded = false;
    public int roundsReached = 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetWin(int rounds)
    {
        playerWon = true;
        gameEnded = true;
        roundsReached = rounds;
    }

    public void SetLose(int rounds)
    {
        playerWon = false;
        gameEnded = true;
        roundsReached = rounds;
    }

    public void Reset()
    {
        playerWon = false;
        gameEnded = false;
        roundsReached = 0;
    }
}