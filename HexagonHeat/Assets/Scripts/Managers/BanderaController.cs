using System.Collections;
using UnityEngine;

/// <summary>
/// Controls the flag display for Albion (moderator)
/// Shows the safe color flag each round with smooth animation
/// </summary>
public class BanderaController : MonoBehaviour
{
    [Header("Flag Materials")]
    [SerializeField] private Material matAmarillo;
    [SerializeField] private Material matAzul;
    [SerializeField] private Material matBlanco;
    [SerializeField] private Material matRojo;
    [SerializeField] private Material matRosado;
    [SerializeField] private Material matVerde;
    [SerializeField] private Material matVioleta;

    [Header("Flag Components")]
    [Tooltip("Renderer of the flag (the cloth part)")]
    [SerializeField] private Renderer flagRenderer;

    [Tooltip("Animator of Albion/Podium")]
    [SerializeField] private Animator animator;

    [Tooltip("The flag object to enable/disable")]
    [SerializeField] private GameObject banderaObject; // ⭐ NUEVO: Objeto bandera

    [Header("Animation Timing")]
    [Tooltip("Delay before showing flag (should match animation)")]
    [SerializeField] private float delayBeforeShow = 0.5f; // Delay antes de mostrar

    [Tooltip("Duration of fade in effect")]
    [SerializeField] private float fadeInDuration = 0.3f; // Duración del fade

    // Animation parameter name (from your animation)
    private readonly int raiseFlagHash = Animator.StringToHash("RaiseFlag");

    // Private variables
    private CanvasGroup canvasGroup; // ⭐ NUEVO: Para fade in suave
    private Coroutine showFlagCoroutine; // ⭐ NUEVO: Controlar corrutina

    #region Unity Lifecycle

    private void Awake()
    {
        // Si no está asignado, buscar automáticamente
        if (banderaObject == null)
        {
            banderaObject = transform.Find("Bandera_Palo")?.gameObject;
        }

        // Buscar o crear CanvasGroup para fade
        canvasGroup = banderaObject?.GetComponent<CanvasGroup>();
        if (banderaObject != null && canvasGroup == null)
        {
            // Si no tiene CanvasGroup, agregar uno
            canvasGroup = banderaObject.AddComponent<CanvasGroup>();
        }

        // Asegurar que la bandera está desactivada al inicio
        if (banderaObject != null)
        {
            banderaObject.SetActive(false);
            Debug.Log("[FLAG] Bandera inicializada - desactivada");
        }
    }

    #endregion

    /// <summary>
    /// Show flag with the specified color and play animation
    /// </summary>
    public void ShowFlag(HexagonColor color)
    {
        // Detener corrutina anterior si está corriendo
        if (showFlagCoroutine != null)
        {
            StopCoroutine(showFlagCoroutine);
        }

        // Change flag material to match the safe color
        ChangeFlagColor(color);

        // Trigger raise flag animation
        if (animator != null)
        {
            animator.SetTrigger(raiseFlagHash);
            Debug.Log($"[FLAG] Triggering RaiseFlag animation for color: {color}");
        }
        else
        {
            Debug.LogWarning("[FLAG] Animator not assigned!");
        }

        // ⭐ NUEVO: Iniciar corrutina para mostrar bandera con delay y fade
        showFlagCoroutine = StartCoroutine(ShowFlagWithDelay(color));
    }

    /// <summary>
    /// ⭐ NUEVO: Mostrar bandera con delay y fade in suave
    /// </summary>
    private IEnumerator ShowFlagWithDelay(HexagonColor color)
    {
        // Esperar a que la mano esté levantada (delay configurable)
        yield return new WaitForSeconds(delayBeforeShow);

        if (banderaObject == null)
        {
            Debug.LogWarning("[FLAG] Bandera object not found!");
            yield break;
        }

        // Activar la bandera (hacerla visible)
        banderaObject.SetActive(true);

        // ⭐ NUEVO: Fade in suave
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f; // Empezar invisible

            float elapsedTime = 0f;
            while (elapsedTime < fadeInDuration)
            {
                elapsedTime += Time.deltaTime;
                canvasGroup.alpha = Mathf.Clamp01(elapsedTime / fadeInDuration);
                yield return null;
            }

            canvasGroup.alpha = 1f; // Asegurar que es completamente visible
            Debug.Log($"[FLAG] Bandera visible con fade in - Color: {color}");
        }
        else
        {
            Debug.Log($"[FLAG] Bandera activada (sin fade) - Color: {color}");
        }
    }

    /// <summary>
    /// Hide the flag immediately
    /// </summary>
    public void HideFlag()
    {
        if (showFlagCoroutine != null)
        {
            StopCoroutine(showFlagCoroutine);
        }

        if (banderaObject != null)
        {
            banderaObject.SetActive(false);
            Debug.Log("[FLAG] Bandera ocultada");
        }
    }

    /// <summary>
    /// Change the flag material to match the color
    /// </summary>
    private void ChangeFlagColor(HexagonColor color)
    {
        if (flagRenderer == null)
        {
            Debug.LogWarning("[FLAG] Flag renderer not assigned!");
            return;
        }

        Material selectedMaterial = null;
        switch (color)
        {
            case HexagonColor.Amarillo:
                selectedMaterial = matAmarillo;
                break;
            case HexagonColor.Azul:
                selectedMaterial = matAzul;
                break;
            case HexagonColor.Blanco:
                selectedMaterial = matBlanco;
                break;
            case HexagonColor.Rojo:
                selectedMaterial = matRojo;
                break;
            case HexagonColor.Rosado:
                selectedMaterial = matRosado;
                break;
            case HexagonColor.Verde:
                selectedMaterial = matVerde;
                break;
            case HexagonColor.Violeta:
                selectedMaterial = matVioleta;
                break;
        }

        if (selectedMaterial != null)
        {
            flagRenderer.material = selectedMaterial;
            Debug.Log($"[FLAG] Changed flag color to: {color}");
        }
        else
        {
            Debug.LogWarning($"[FLAG] No material assigned for color: {color}");
        }
    }
}