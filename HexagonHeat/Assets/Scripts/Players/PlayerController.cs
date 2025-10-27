using UnityEngine;

/// <summary>
/// Controla el movimiento del jugador usando WASD
/// Detecta cuando cae de hexágonos y maneja animaciones
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class PlayerController : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [Tooltip("Velocidad de movimiento normal")]
    [SerializeField] private float moveSpeed = 5f;

    [Tooltip("Velocidad al correr (presionando Shift)")]
    [SerializeField] private float runSpeed = 8f;

    [Tooltip("Suavidad de rotación del personaje")]
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Detección de Suelo")]
    [Tooltip("Punto de verificación del suelo (crea un Empty GameObject bajo los pies)")]
    [SerializeField] private Transform groundCheck;

    [Tooltip("Radio de detección del suelo")]
    [SerializeField] private float groundCheckRadius = 0.3f;

    [Tooltip("Layer del suelo (asignar layer de hexágonos)")]
    [SerializeField] private LayerMask groundLayer;

    [Header("Componentes")]
    [Tooltip("Referencia al Animator del personaje")]
    [SerializeField] private Animator animator;

    [Header("Configuración de Cámara")]
    [Tooltip("Referencia a la cámara principal para movimiento relativo")]
    [SerializeField] private Transform mainCamera;

    // Variables privadas
    private Rigidbody rb;
    private Vector3 moveDirection;
    private bool isGrounded;
    private bool hasFallen = false;
    private bool isRunning = false;

    // Nombres de parámetros del Animator (usar hash para mejor rendimiento)
    private readonly int speedHash = Animator.StringToHash("Speed");
    private readonly int isMovingHash = Animator.StringToHash("IsMoving");
    private readonly int isGroundedHash = Animator.StringToHash("IsGrounded");

    #region Unity Lifecycle

    private void Awake()
    {
        // Configurar Rigidbody
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation; // Evitar rotaciones no deseadas
        rb.useGravity = true;

        // Buscar animator si no está asignado
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        // Buscar cámara si no está asignada
        if (mainCamera == null)
        {
            mainCamera = Camera.main?.transform;
        }

        // Crear GroundCheck automáticamente si no existe
        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -1f, 0); // Ajustar según tu modelo
            groundCheck = groundCheckObj.transform;
            Debug.LogWarning("[PLAYER] GroundCheck creado automáticamente. Ajusta su posición en el Inspector.");
        }

        Debug.Log($"[PLAYER] {gameObject.name} inicializado - Controles: WASD para mover, Shift para correr");
    }

    private void Update()
    {
        CheckGroundStatus();
        HandleInput();
        UpdateAnimations();
        CheckIfFalling();
    }

    private void FixedUpdate()
    {
        MovePlayer();
    }

    #endregion

    #region Input Handling

    /// <summary>
    /// Maneja el input del jugador (WASD)
    /// </summary>
    private void HandleInput()
    {
        // Obtener input horizontal (A/D o Flechas Izq/Der)
        float horizontal = Input.GetAxisRaw("Horizontal");

        // Obtener input vertical (W/S o Flechas Arriba/Abajo)
        float vertical = Input.GetAxisRaw("Vertical");

        // Detectar si está corriendo (Shift)
        isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        // Calcular dirección de movimiento
        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

        // Convertir a dirección relativa a la cámara
        if (inputDirection.magnitude >= 0.1f)
        {
            // Si hay cámara, mover relativo a su rotación
            if (mainCamera != null)
            {
                float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mainCamera.eulerAngles.y;
                moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            }
            else
            {
                // Si no hay cámara, usar dirección local
                moveDirection = transform.TransformDirection(inputDirection);
            }
        }
        else
        {
            moveDirection = Vector3.zero;
        }
    }

    #endregion

    #region Movement

    /// <summary>
    /// Mueve al jugador en la dirección calculada
    /// </summary>
    private void MovePlayer()
    {
        if (moveDirection.magnitude >= 0.1f && isGrounded && !hasFallen)
        {
            // Calcular velocidad (normal o corriendo)
            float currentSpeed = isRunning ? runSpeed : moveSpeed;

            // Mover usando Rigidbody (mantiene física)
            Vector3 targetVelocity = moveDirection * currentSpeed;
            targetVelocity.y = rb.velocity.y; // Mantener velocidad vertical (gravedad)
            rb.velocity = targetVelocity;

            // Rotar hacia la dirección de movimiento
            if (moveDirection != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    targetRotation,
                    Time.deltaTime * rotationSpeed
                );
            }
        }
        else if (isGrounded)
        {
            // Si no se está moviendo, frenar gradualmente
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
        }
    }

    #endregion

    #region Ground Detection

    /// <summary>
    /// Verifica si el jugador está tocando el suelo (hexágonos)
    /// </summary>
    private void CheckGroundStatus()
    {
        if (groundCheck == null)
        {
            // Fallback: usar posición del jugador
            isGrounded = Physics.CheckSphere(
                transform.position,
                groundCheckRadius,
                groundLayer
            );
        }
        else
        {
            isGrounded = Physics.CheckSphere(
                groundCheck.position,
                groundCheckRadius,
                groundLayer
            );
        }
    }

    /// <summary>
    /// Verifica si el jugador está cayendo y debe ser eliminado
    /// </summary>
    private void CheckIfFalling()
    {
        // Si no está en el suelo y está cayendo (velocidad Y negativa)
        if (!isGrounded && rb.velocity.y < -1f && !hasFallen)
        {
            OnPlayerFell();
        }
    }

    /// <summary>
    /// Llamado cuando el jugador cae de un hexágono
    /// </summary>
    private void OnPlayerFell()
    {
        hasFallen = true;

        Debug.Log($"[PLAYER] {gameObject.name} cayó del hexágono! 💀");

        // Aquí puedes agregar:
        // - Animación de muerte
        // - Efectos de partículas
        // - Sonidos
        // - Notificar al GameManager
        // - Pantalla de Game Over

        // Desactivar controles
        enabled = false;

        // Opcional: Destruir jugador después de un delay
        // Destroy(gameObject, 2f);
    }

    #endregion

    #region Animations

    /// <summary>
    /// Actualiza los parámetros del Animator según el estado del jugador
    /// </summary>
    private void UpdateAnimations()
    {
        if (animator == null) return;

        // Calcular velocidad actual
        float speed = new Vector3(rb.velocity.x, 0f, rb.velocity.z).magnitude;

        // Actualizar parámetros del Animator
        animator.SetFloat(speedHash, speed);
        animator.SetBool(isMovingHash, speed > 0.1f);
        animator.SetBool(isGroundedHash, isGrounded);

        // Debug info
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log($"[PLAYER] Speed: {speed:F2} | Moving: {speed > 0.1f} | Grounded: {isGrounded}");
        }
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Verificar si el jugador está en el suelo
    /// </summary>
    public bool IsGrounded()
    {
        return isGrounded;
    }

    /// <summary>
    /// Verificar si el jugador cayó
    /// </summary>
    public bool HasFallen()
    {
        return hasFallen;
    }

    /// <summary>
    /// Resetear el jugador (útil para reiniciar el juego)
    /// </summary>
    public void ResetPlayer(Vector3 spawnPosition)
    {
        hasFallen = false;
        transform.position = spawnPosition;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        enabled = true;

        Debug.Log("[PLAYER] Jugador reseteado");
    }

    #endregion

    #region Debug Visualization

    /// <summary>
    /// Dibuja gizmos en el editor para visualizar detección de suelo
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        // Dibujar esfera de detección de suelo
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
        else
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position, groundCheckRadius);
        }

        // Dibujar dirección de movimiento
        if (moveDirection != Vector3.zero)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, moveDirection * 2f);
        }
    }

    #endregion
}
