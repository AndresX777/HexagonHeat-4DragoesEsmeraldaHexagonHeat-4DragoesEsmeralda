using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System; // ⭐ para eventos

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
    [Tooltip("Punto de verificación del suelo")]
    [SerializeField] private Transform groundCheck;

    [Tooltip("Radio de detección del suelo")]
    [SerializeField] private float groundCheckRadius = 0.3f;

    [Tooltip("Layer del suelo (asignar layer de hexágonos)")]
    [SerializeField] private LayerMask groundLayer;

    [Header("Detección de Caída")]
    [Tooltip("Altura mínima antes de considerar que cayó al agua")]
    [SerializeField] private float fallThreshold = -5f;

    [Header("Componentes")]
    [Tooltip("Referencia al Animator del personaje")]
    [SerializeField] private Animator animator;

    [Header("Configuración de Cámara")]
    [Tooltip("Referencia a la cámara principal")]
    [SerializeField] private Transform mainCamera;

    // ⭐ EVENTO: Se dispara cuando el jugador cae
    public static event Action OnPlayerDied;

    // Variables privadas
    private Rigidbody rb;
    private Vector3 moveDirection;
    private bool isGrounded;
    private bool hasFallen = false;
    private bool isRunning = false;
    private bool isAlive = true; // ⭐ NUEVO: Estado del jugador

    // Hash de animaciones
    private readonly int speedHash = Animator.StringToHash("Speed");
    private readonly int isMovingHash = Animator.StringToHash("IsMoving");
    private readonly int isGroundedHash = Animator.StringToHash("IsGrounded");
    private readonly int fallingHash = Animator.StringToHash("IsFalling"); // ⭐ NUEVO

    #region Unity Lifecycle

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;
        rb.useGravity = true;

        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }

        if (mainCamera == null)
        {
            mainCamera = Camera.main?.transform;
        }

        if (groundCheck == null)
        {
            GameObject groundCheckObj = new GameObject("GroundCheck");
            groundCheckObj.transform.SetParent(transform);
            groundCheckObj.transform.localPosition = new Vector3(0, -1f, 0);
            groundCheck = groundCheckObj.transform;
        }

        Debug.Log($"[PLAYER] {gameObject.name} inicializado");
    }

    private void Update()
    {
        if (!isAlive) return; // ⭐ No procesar si está muerto

        CheckGroundStatus();
        HandleInput();
        UpdateAnimations();
        CheckIfFalling();
    }

    private void FixedUpdate()
    {
        if (!isAlive) return; // ⭐ No procesar si está muerto
        MovePlayer();
    }

    #endregion

    #region Input Handling

    private void HandleInput()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);

        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;

        if (inputDirection.magnitude >= 0.1f)
        {
            if (mainCamera != null)
            {
                float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg + mainCamera.eulerAngles.y;
                moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            }
            else
            {
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

    private void MovePlayer()
    {
        if (moveDirection.magnitude >= 0.1f && isGrounded && !hasFallen)
        {
            float currentSpeed = isRunning ? runSpeed : moveSpeed;

            Vector3 targetVelocity = moveDirection * currentSpeed;
            targetVelocity.y = rb.velocity.y;
            rb.velocity = targetVelocity;

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
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
        }
    }

    #endregion

    #region Ground Detection

    private void CheckGroundStatus()
    {
        if (groundCheck == null)
        {
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
    /// ⭐ MODIFICADO: Verifica si el jugador cayó al agua
    /// </summary>
    private void CheckIfFalling()
    {
        // Método 1: Detectar por velocidad Y negativa cuando no está en suelo
        if (!isGrounded && rb.velocity.y < -2f && !hasFallen)
        {
            // Activar animación de caída
            if (animator != null)
            {
                animator.SetBool(fallingHash, true);
            }
        }

        // Método 2: Detectar por altura (más confiable)
        if (transform.position.y < fallThreshold && !hasFallen)
        {
            OnPlayerFell();
        }
    }

    /// <summary>
    /// ⭐ MODIFICADO: Llamado cuando el jugador cae al agua
    /// </summary>
    private void OnPlayerFell()
    {
        if (hasFallen) return; // Evitar múltiples llamadas

        hasFallen = true;
        isAlive = false;

        Debug.Log($"[PLAYER] ¡{gameObject.name} cayó al agua! 💀");

        // Desactivar controles
        rb.velocity = Vector3.zero;

        // Activar animación de caída/muerte
        if (animator != null)
        {
            animator.SetBool(fallingHash, true);
        }

        // ⭐ NOTIFICAR AL GAME MANAGER
        OnPlayerDied?.Invoke();

        // Opcional: Desactivar el jugador después de un tiempo
        StartCoroutine(DisableAfterDelay(1.5f));
    }

    private IEnumerator DisableAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        gameObject.SetActive(false);
    }

    #endregion

    #region Animations

    private void UpdateAnimations()
    {
        if (animator == null) return;

        float speed = new Vector3(rb.velocity.x, 0f, rb.velocity.z).magnitude;

        animator.SetFloat(speedHash, speed);
        animator.SetBool(isMovingHash, speed > 0.1f);
        animator.SetBool(isGroundedHash, isGrounded);
    }

    #endregion

    #region Public Methods

    public bool IsGrounded()
    {
        return isGrounded;
    }

    public bool HasFallen()
    {
        return hasFallen;
    }

    /// <summary>
    /// ⭐ NUEVO: Verificar si el jugador está vivo
    /// </summary>
    public bool IsAlive()
    {
        return isAlive;
    }

    /// <summary>
    /// ⭐ MODIFICADO: Resetear el jugador
    /// </summary>
    public void ResetPlayer(Vector3 spawnPosition)
    {
        hasFallen = false;
        isAlive = true;
        transform.position = spawnPosition;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        gameObject.SetActive(true);
        enabled = true;

        if (animator != null)
        {
            animator.SetBool(fallingHash, false);
        }

        Debug.Log("[PLAYER] Jugador reseteado");
    }

    #endregion

    #region Debug Visualization

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = isGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }

        // Dibujar línea del threshold de caída
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(
            new Vector3(transform.position.x - 2f, fallThreshold, transform.position.z),
            new Vector3(transform.position.x + 2f, fallThreshold, transform.position.z)
        );

        if (moveDirection != Vector3.zero)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(transform.position, moveDirection * 2f);
        }
    }

    #endregion
}