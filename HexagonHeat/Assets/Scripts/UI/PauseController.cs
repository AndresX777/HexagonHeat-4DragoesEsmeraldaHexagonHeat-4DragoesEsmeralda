using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseController : MonoBehaviour
{
    public GameObject panelPausa;
    private bool juegoPausado = false;

    void Update()
    {
        // Detects ESC
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (juegoPausado)
                Reanudar();
            else
                Pausar();
        }
    }

    public void Pausar()
    {
        panelPausa.SetActive(true);
        Time.timeScale = 0f;
        juegoPausado = true;


        if (LanguageManager.Instance != null)
            LanguageManager.Instance.UpdateAllLocalizedSprites();
    }

    public void Reanudar()
    {
        panelPausa.SetActive(false);
        Time.timeScale = 1f;
        juegoPausado = false;
    }

    public void AbrirOpciones()
    {
  
        panelPausa.SetActive(false);

  
        Time.timeScale = 1f;

        MenuPrincipalController menu = FindObjectOfType<MenuPrincipalController>();

        if (menu != null)
        {
            menu.panelOpciones.SetActive(true);
        }

        juegoPausado = false;
    }

    public void MenuPrincipal()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("UI_Menu");
    }

    public void SalirDelJuego()
    {
        Application.Quit();
    }
}