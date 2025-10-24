using UnityEngine;

/// <summary>
/// Controls individual hexagon behavior: color state and falling mechanics
/// </summary>
public class HexagonController : MonoBehaviour
{
    [Header("Hexagon Settings")]
    [Tooltip("Color identifier for this hexagon")]
    [SerializeField] private HexagonColor hexColor;

    [Tooltip("Time to wait before regenerating after falling")]
    [SerializeField] private float regenerationDelay = 1f;

    [Header("Visual Feedback")]
    [Tooltip("Material when hexagon is safe")]
    [SerializeField] private Material safeMaterial;

    [Tooltip("Material when hexagon is falling")]
    [SerializeField] private Material dangerMaterial;

    // Private variables
    private Renderer hexRenderer;
    private Collider hexCollider;
    private bool isSafe = true;
    private bool isFalling = false;
    private Rigidbody rb;
    private Vector3 originalPosition;
    private Quaternion originalRotation;
    private Material originalMaterial;

    #region Unity Lifecycle

    private void Awake()
    {
        hexRenderer = GetComponent<Renderer>();
        hexCollider = GetComponent<Collider>();

        // Save original state
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        if (hexRenderer != null && hexRenderer.material != null)
        {
            originalMaterial = hexRenderer.material;
        }

        // Add Rigidbody if not present
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody>();
        }

        // Configure Rigidbody
        rb.isKinematic = true; // Start as kinematic
        rb.useGravity = false; // Gravity controlled manually

        // DEBUG: Log hexagon initialization
        Debug.Log($"[INIT] {gameObject.name} initialized with color: {hexColor}");
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Set this hexagon as safe or unsafe
    /// </summary>
    public void SetSafeState(bool safe)
    {
        isSafe = safe;

        // DEBUG: Log state change
        Debug.Log($"[STATE] {gameObject.name} ({hexColor}) set to: {(safe ? "SAFE ✅" : "UNSAFE ❌")}");

        if (!safe && !isFalling)
        {
            Debug.Log($"[FALLING] {gameObject.name} ({hexColor}) starting to fall!");
            StartFalling();
        }
        else if (safe)
        {
            Debug.Log($"[SAFE] {gameObject.name} ({hexColor}) is the safe hexagon!");
        }
    }

    /// <summary>
    /// Get the color of this hexagon
    /// </summary>
    public HexagonColor GetColor()
    {
        return hexColor;
    }

    /// <summary>
    /// Check if hexagon is currently safe
    /// </summary>
    public bool IsSafe()
    {
        return isSafe;
    }

    /// <summary>
    /// Check if hexagon is falling
    /// </summary>
    public bool IsFalling()
    {
        return isFalling;
    }

    /// <summary>
    /// Regenerate the hexagon to its original position
    /// </summary>
    public void Regenerate()
    {
        // Stop falling
        isFalling = false;
        isSafe = true;

        // Reset physics
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        rb.useGravity = false;

        // Reset position and rotation
        transform.position = originalPosition;
        transform.rotation = originalRotation;

        // Re-enable collider
        if (hexCollider != null)
        {
            hexCollider.enabled = true;
        }

        // Reset material
        if (hexRenderer != null && originalMaterial != null)
        {
            hexRenderer.material = originalMaterial;
        }

        // Re-enable the game object if it was disabled
        gameObject.SetActive(true);

        Debug.Log($"[REGEN] {gameObject.name} ({hexColor}) regenerated!");
    }

    #endregion

    #region Falling Mechanics

    /// <summary>
    /// Start the falling animation
    /// </summary>
    private void StartFalling()
    {
        isFalling = true;

        // Change visual to danger state
        if (dangerMaterial != null && hexRenderer != null)
        {
            hexRenderer.material = dangerMaterial;
        }

        // Enable physics
        rb.isKinematic = false;
        rb.useGravity = true;

        // Disable collider after a short delay
        Invoke(nameof(DisableCollider), 0.5f);

        // Schedule regeneration instead of destroying
        Invoke(nameof(Regenerate), regenerationDelay);

        Debug.Log($"[PHYSICS] {gameObject.name} ({hexColor}) physics enabled - falling now!");
    }

    /// <summary>
    /// Disable collider so players fall through
    /// </summary>
    private void DisableCollider()
    {
        if (hexCollider != null)
        {
            hexCollider.enabled = false;
            Debug.Log($"[COLLIDER] {gameObject.name} ({hexColor}) collider disabled");
        }
    }

    #endregion

    #region Debug

    private void OnDrawGizmos()
    {
        // Draw color indicator in Scene view
        Gizmos.color = isSafe ? Color.green : Color.red;
        Gizmos.DrawWireCube(transform.position + Vector3.up * 0.5f, Vector3.one * 0.5f);
    }

    #endregion
}

/// <summary>
/// Enum for hexagon colors (matches your game design)
/// </summary>
public enum HexagonColor
{
    Amarillo,  // Yellow
    Azul,      // Blue
    Blanco,    // White
    Rojo,      // Red
    Rosado,    // Pink
    Verde,     // Green
    Violeta    // Violet
}