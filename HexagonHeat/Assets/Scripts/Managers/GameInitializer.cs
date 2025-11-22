using UnityEngine;
using UnityEngine.SceneManagement;

public class GameInitializer : MonoBehaviour
{
    [Header("Configuración de Desarrollo")]
    [Tooltip("Activar para saltar el menú durante pruebas (solo para desarrollo)")]
    public bool saltarMenuEnDesarrollo = false;

    void Awake()
    {
        // Si está marcado para desarrollo, no cargar el menú
        if (saltarMenuEnDesarrollo)
        {
            Debug.Log("Modo desarrollo: saltando menú");
            return;
        }

        // Verificar si venimos desde el menú
        if (!PlayerPrefs.HasKey("FromMenu"))
        {
            // Si no venimos del menú, cargar el menú primero
            Debug.Log("Iniciando desde escena de juego, cargando menú...");
            SceneManager.LoadScene("UI_Menu");
        }
        else
        {
            // Limpiar la marca para la próxima vez
            PlayerPrefs.DeleteKey("FromMenu");
            Debug.Log("Juego iniciado correctamente desde el menú");
        }
    }
}