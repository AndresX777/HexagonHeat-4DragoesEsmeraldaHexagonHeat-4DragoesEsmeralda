using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; // Importar sistema de navegación para IA

/// <summary>
/// Controla el movimiento, pathfinding y animaciones de un NPC
/// </summary>
[RequireComponent(typeof(NavMeshAgent))] // Requiere componente NavMeshAgent automáticamente
public class NPCController : MonoBehaviour
{
    [Header("Configuración de IA")]
    [Tooltip("Velocidad de movimiento del NPC")]
    [SerializeField] private float moveSpeed = 3.5f; // Velocidad de desplazamiento

    [Tooltip("Cada cuántos segundos cambia de destino")]
    [SerializeField] private float destinationChangeTime = 5f; // Tiempo entre cambios de ruta

    [Tooltip("Radio de deambulación desde la posición inicial")]
    [SerializeField] private float wanderRadius = 10f; // Área donde puede moverse

    [Header("Ground Detection")]
    [Tooltip("Transform point to check if NPC is grounded")]
    [SerializeField] private Transform groundCheck; // Punto de verificación del suelo

    [Tooltip("Radius of the ground check sphere")]
    [SerializeField] private float groundCheckRadius = 0.3f; // Radio de detección

    [Tooltip("Layer mask for what counts as ground")]
    [SerializeField] private LayerMask groundLayer; // Capa del suelo (hexágonos)

    [Header("Componentes")]
    [Tooltip("Referencia al componente Animator")]
    [SerializeField] private Animator animator; // Controlador de animaciones

    // Variables privadas
    private NavMeshAgent agent; // Referencia al agente de navegación
    private Vector3 startPosition; // Posición inicial del NPC
    private float destinationTimer; // Temporizador para cambiar destino
    private bool isMoving; // Estado de movimiento
    private bool isGrounded; // Estado si está en el suelo

    // Nombres de parámetros del Animator (mejor rendimiento con hash)
    private readonly int speedHash = Animator.StringToHash("Speed"); // Hash para velocidad
    private readonly int isMovingHash = Animator.StringToHash("IsMoving"); // Hash para estado movimiento
    private readonly int isGroundedHash = Animator.StringToHash("IsGrounded"); // Hash para estado en suelo

    #region Métodos del Ciclo de Unity

    // Método que se ejecuta al crear el objeto
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>(); // Obtener referencia al NavMeshAgent
        startPosition = transform.position; // Guardar posición inicial

        // Configurar NavMeshAgent
        agent.speed = moveSpeed; // Establecer velocidad
        agent.angularSpeed = 360f; // Velocidad de rotación
        agent.acceleration = 8f; // Aceleración
        agent.stoppingDistance = 0.5f; // Distancia para detenerse

        // Buscar automáticamente animator si no está asignado
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>(); // Buscar en objetos hijos
        }
    }

    // Método que se ejecuta al inicio del juego
    private void Start()
    {
        SetRandomDestination(); // Establecer destino inicial
    }

    // Método que se ejecuta cada frame
    private void Update()
    {
        CheckGroundStatus(); // Verificar si está en el suelo
        UpdateDestination(); // Actualizar destino del NPC
        UpdateAnimations(); // Actualizar animaciones
        CheckIfFalling(); // Verificar si está cayendo
    }

    #endregion

    #region Ground Detection

    /// <summary>
    /// Check if NPC is touching the ground (hexagon)
    /// </summary>
    private void CheckGroundStatus()
    {
        if (groundCheck == null)
        {
            // Fallback: use NPC's position if ground check not set
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
    /// Check if NPC is falling and should be eliminated
    /// </summary>
    private void CheckIfFalling()
    {
        // If not grounded and falling (Y velocity negative)
        if (!isGrounded && agent.velocity.y < -1f)
        {
            OnNPCFell(); // Handle NPC elimination
        }
    }

    /// <summary>
    /// Called when NPC falls off a hexagon
    /// </summary>
    private void OnNPCFell()
    {
        // Disable agent to prevent further movement
        agent.enabled = false;

        // Here you can add:
        // - Death animation
        // - Particle effects
        // - Sound effects
        // - Remove from game manager

        Debug.Log($"{gameObject.name} fell off the hexagon!");

        // Destroy NPC after a delay
        Destroy(gameObject, 2f);
    }

    #endregion

    #region Comportamiento de IA

    /// <summary>
    /// Actualizar destino basado en temporizador
    /// </summary>
    private void UpdateDestination()
    {
        destinationTimer -= Time.deltaTime; // Reducir temporizador

        // Si el temporizador llega a cero
        if (destinationTimer <= 0f)
        {
            SetRandomDestination(); // Establecer nuevo destino aleatorio
            destinationTimer = destinationChangeTime; // Reiniciar temporizador
        }

        // Verificar si llegó al destino
        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            isMoving = false; // Ya no se está moviendo
        }
        else
        {
            isMoving = true; // Todavía está en movimiento
        }
    }

    /// <summary>
    /// Establecer un destino aleatorio dentro del radio de deambulación
    /// </summary>
    private void SetRandomDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius; // Dirección aleatoria
        randomDirection += startPosition; // Centrar en posición inicial

        NavMeshHit hit; // Variable para guardar resultado de búsqueda
        // Buscar posición válida en el NavMesh
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position); // Establecer nuevo destino
            isMoving = true; // Activar estado de movimiento
        }
    }

    #endregion

    #region Animaciones

    /// <summary>
    /// Actualizar parámetros del animator basado en estado del NPC
    /// </summary>
    private void UpdateAnimations()
    {
        if (animator == null) return; // Salir si no hay animator

        float speed = agent.velocity.magnitude; // Calcular velocidad actual
        animator.SetFloat(speedHash, speed); // Pasar velocidad al animator
        animator.SetBool(isMovingHash, isMoving && speed > 0.1f); // Pasar estado movimiento
        animator.SetBool(isGroundedHash, isGrounded); // Pasar estado en suelo

        // Rotar NPC para mirar hacia la dirección de movimiento
        if (agent.velocity.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(agent.velocity.normalized); // Calcular rotación objetivo
            transform.rotation = Quaternion.Slerp( // Rotar suavemente
                transform.rotation,
                targetRotation,
                Time.deltaTime * 5f // Velocidad de rotación
            );
        }
    }

    #endregion

    #region Métodos Públicos (para control externo)

    /// <summary>
    /// Hacer que el NPC se mueva a una posición específica
    /// </summary>
    public void MoveToPosition(Vector3 position)
    {
        agent.SetDestination(position); // Establecer destino específico
        isMoving = true; // Activar movimiento
        destinationTimer = destinationChangeTime; // Reiniciar temporizador
    }

    /// <summary>
    /// Detener el movimiento del NPC
    /// </summary>
    public void StopMoving()
    {
        agent.ResetPath(); // Limpiar ruta actual
        isMoving = false; // Desactivar movimiento
    }

    /// <summary>
    /// Verificar si el NPC se está moviendo actualmente
    /// </summary>
    public bool IsMoving()
    {
        return isMoving; // Devolver estado de movimiento
    }

    /// <summary>
    /// Check if NPC is currently grounded
    /// </summary>
    public bool IsGrounded()
    {
        return isGrounded; // Return grounded state
    }

    #endregion

    #region Visualización de Debug

    // Método para dibujar gizmos en el editor cuando el objeto está seleccionado
    private void OnDrawGizmosSelected()
    {
        // Dibujar radio de deambulación
        Gizmos.color = Color.cyan; // Color cian
        Gizmos.DrawWireSphere(Application.isPlaying ? startPosition : transform.position, wanderRadius); // Círculo del área

        // Dibujar ruta actual si se está moviendo
        if (agent != null && agent.hasPath)
        {
            Gizmos.color = Color.yellow; // Color amarillo para la línea
            Gizmos.DrawLine(transform.position, agent.destination); // Línea hacia destino

            Gizmos.color = Color.red; // Color rojo para el destino
            Gizmos.DrawWireSphere(agent.destination, 0.5f); // Esfera en el destino
        }

        // Draw ground check sphere
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
    }

    #endregion
}