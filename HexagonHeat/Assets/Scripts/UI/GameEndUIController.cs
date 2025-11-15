using UnityEngine;

public class GameEndUIController : MonoBehaviour
{
    [Header("Panels")]
    public GameObject winPanel;
    public GameObject losePanel;

    private MenuPrincipalController menu;

    private void Start()
    {
        winPanel.SetActive(false);
        losePanel.SetActive(false);

        menu = FindObjectOfType<MenuPrincipalController>();
    }


    public void ShowWin()
    {
        winPanel.SetActive(true);
        losePanel.SetActive(false);
    }

    public void ShowLose()
    {
        losePanel.SetActive(true);
        winPanel.SetActive(false);
    }


    public void VolverAJugar()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Escenario");
    }

    public void IrAlMenuPrincipal()
    {
        winPanel.SetActive(false);
        losePanel.SetActive(false);

        menu.panelMenu.SetActive(true);
    }

    public void AbrirCreditos()
    {
        winPanel.SetActive(false);
        losePanel.SetActive(false);

        menu.panelCreditos.SetActive(true);
    }

    public void SalirDelJuego()
    {
        Application.Quit();
    }
}


