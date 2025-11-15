using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class MenuPrincipalController : MonoBehaviour
{
    // Main Panels
    public GameObject panelMenu;
    public GameObject panelCreditos;
    public GameObject panelOpciones;
    public GameObject panelSonido;
    public GameObject panelControles;
    public GameObject panelAccesibilidad;
    public GameObject panelIdioma;
    private AccessibilityFilter filter;

    private void ActivarPanel(GameObject panelAActivar, GameObject panelADesactivar)
    {
        panelADesactivar.SetActive(false);
        panelAActivar.SetActive(true);

        LanguageManager.Instance.UpdateAllLocalizedSprites();
    }
    //Filter
    private void Start()
    {
        filter = Camera.main.GetComponent<AccessibilityFilter>();
    }

    // Sliders
    public Slider sliderSonido;
    public Slider sliderEfectos;

    // Accessiility
    public bool contrasteAlto = false;
    public bool modoDaltonico = false;

    public TextMeshProUGUI textoBotonContraste;
    public TextMeshProUGUI textoBotonDaltonico;

    // Countdown
    public GameStartController gameStartController;

    public void Jugar()
    {
        panelMenu.SetActive(false);
        gameStartController.StartCountdown();
    }
    // Menu options
    public void Opciones()
    {
        ActivarPanel(panelOpciones, panelMenu);
    }

    public void RegresarDeOpciones()
    {
        ActivarPanel(panelMenu, panelOpciones);
    }

    // Menu sound

    public void Sonido()
    {
        ActivarPanel(panelSonido, panelOpciones);
    }

    public void RegresarDeSonido()
    {
        ActivarPanel(panelOpciones, panelSonido);
    }

    public void CambiarVolumenGeneral(float valor)
    {
        AudioListener.volume = valor;
    }

    public void CambiarVolumenEfectos(float valor)
    {
        // Space to implement the audio
    }

    // Menu controls

    public void Controles()
    {
        ActivarPanel(panelControles, panelOpciones);
    }

    public void RegresarDeControles()
    {
        ActivarPanel(panelOpciones, panelControles);
    }

    // Menu Accessiility

    public void Accesibilidad()
    {
        ActivarPanel(panelAccesibilidad, panelOpciones);
    }

    public void RegresarDeAccesibilidad()
    {
        ActivarPanel(panelOpciones, panelAccesibilidad);
    }

    public void ToggleContraste()
    {
        contrasteAlto = !contrasteAlto;

        if (contrasteAlto)
            filter.currentFilter = AccessibilityFilter.FilterMode.HighContrast;
        else
            filter.currentFilter = AccessibilityFilter.FilterMode.Normal;

        textoBotonContraste.text = contrasteAlto ? "ON" : "OFF";
    }

    public void ToggleDaltonico()
    {
        modoDaltonico = !modoDaltonico;

        if (modoDaltonico)
            filter.currentFilter = AccessibilityFilter.FilterMode.Deuteranopia;
        else
            filter.currentFilter = AccessibilityFilter.FilterMode.Normal;

        textoBotonDaltonico.text = modoDaltonico ? "ON" : "OFF";
    }


    // Menu Language
    public void Idioma()
    {
        ActivarPanel(panelIdioma, panelOpciones);
    }

    public void SeleccionarIdiomaEspanol()
    {
        LanguageManager.Instance.SetLanguage(LanguageManager.Language.Espanol);
    }

    public void SeleccionarIdiomaIngles()
    {
        LanguageManager.Instance.SetLanguage(LanguageManager.Language.Ingles);
    }

    public void RegresarDeIdioma()
    {
        ActivarPanel(panelOpciones, panelIdioma);
    }

    // Credits and exit

    public void Creditos()
    {
        ActivarPanel(panelCreditos, panelMenu);
    }

    public void Regresar()
    {
        ActivarPanel(panelMenu, panelCreditos);
    }

    public void Salir()
    {
        Application.Quit();
    }
}
