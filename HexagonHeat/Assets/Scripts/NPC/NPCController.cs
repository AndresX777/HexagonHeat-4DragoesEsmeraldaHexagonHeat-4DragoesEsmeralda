using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI; // Importar sistema de navegaci�n para IA

/// <summary>
/// Controla el movimiento, pathfinding y animaciones de un NPC
/// </summary>
[RequireComponent(typeof(NavMeshAgent))] // Requiere componente NavMeshAgent autom�ticamente
public class NPCController : MonoBehaviour
{
    [Header("Configuraci�n de IA")]
    [Tooltip("Velocidad de movimiento del NPC")]
    [SerializeField] private float moveSpeed = 3.5f; // Velocidad de desplazamiento

    [Tooltip("Cada cu�ntos segundos cambia de destino")]
    [SerializeField] private float destinationChangeTime = 5f; // Tiempo entre cambios de ruta

    [Tooltip("Radio de deambulaci�n desde la posici�n inicial")]
    [SerializeField] private float wanderRadius = 10f; // �rea donde puede moverse

    [Header("Ground Detection")]
    [Tooltip("Transform point to check if NPC is grounded")]
    [SerializeField] private Transform groundCheck; // Punto de verificaci�n del suelo

    [Tooltip("Radius of the ground check sphere")]
    [SerializeField] private float groundCheckRadius = 0.3f; // Radio de detecci�n

    [Tooltip("Layer mask for what counts as ground")]
    [SerializeField] private LayerMask groundLayer; // Capa del suelo (hex�gonos)

    [Header("Componentes")]
    [Tooltip("Referencia al componente Animator")]
    [SerializeField] private Animator animator; // Controlador de animaciones

    // Variables privadas
    private NavMeshAgent agent; // Referencia al agente de navegaci�n
    private Vector3 startPosition; // Posici�n inicial del NPC
    private float destinationTimer; // Temporizador para cambiar destino
    private bool isMoving; // Estado de movimiento
    private bool isGrounded; // Estado si est� en el suelo

    // Nombres de par�metros del Animator (mejor rendimiento con hash)
    private readonly int speedHash = Animator.StringToHash("Speed"); // Hash para velocidad
    private readonly int isMovingHash = Animator.StringToHash("IsMoving"); // Hash para estado movimiento
    private readonly int isGroundedHash = Animator.StringToHash("IsGrounded"); // Hash para estado en suelo

    #region M�todos del Ciclo de Unity

    // M�todo que se ejecuta al crear el objeto
    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>(); // Obtener referencia al NavMeshAgent
        startPosition = transform.position; // Guardar posici�n inicial

        // Configurar NavMeshAgent
        agent.speed = moveSpeed; // Establecer velocidad
        agent.angularSpeed = 360f; // Velocidad de rotaci�n
        agent.acceleration = 8f; // Aceleraci�n
        agent.stoppingDistance = 0.5f; // Distancia para detenerse

        // Buscar autom�ticamente animator si no est� asignado
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>(); // Buscar en objetos hijos
        }
    }

    // M�todo que se ejecuta al inicio del juego
    private void Start()
    {
        SetRandomDestination(); // Establecer destino inicial
    }

    // M�todo que se ejecuta cada frame
    private void Update()
    {
        CheckGroundStatus(); // Verificar si est� en el suelo
        UpdateDestination(); // Actualizar destino del NPC
        UpdateAnimations(); // Actualizar animaciones
        CheckIfFalling(); // Verificar si est� cayendo
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

        // Verificar si lleg� al destino
        if (agent.remainingDistance <= agent.stoppingDistance && !agent.pathPending)
        {
            isMoving = false; // Ya no se est� moviendo
        }
        else
        {
            isMoving = true; // Todav�a est� en movimiento
        }
    }

    /// <summary>
    /// Establecer un destino aleatorio dentro del radio de deambulaci�n
    /// </summary>
    private void SetRandomDestination()
    {
        Vector3 randomDirection = Random.insideUnitSphere * wanderRadius; // Direcci�n aleatoria
        randomDirection += startPosition; // Centrar en posici�n inicial

        NavMeshHit hit; // Variable para guardar resultado de b�squeda
        // Buscar posici�n v�lida en el NavMesh
        if (NavMesh.SamplePosition(randomDirection, out hit, wanderRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position); // Establecer nuevo destino
            isMoving = true; // Activar estado de movimiento
        }
    }

    #endregion

    #region Animaciones

    /// <summary>
    /// Actualizar par�metros del animator basado en estado del NPC
    /// </summary>
    private void UpdateAnimations()
    {
        if (animator == null) return; // Salir si no hay animator

        float speed = agent.velocity.magnitude; // Calcular velocidad actual
        animator.SetFloat(speedHash, speed); // Pasar velocidad al animator
        animator.SetBool(isMovingHash, isMoving && speed > 0.1f); // Pasar estado movimiento
        animator.SetBool(isGroundedHash, isGrounded); // Pasar estado en suelo

        // Rotar NPC para mirar hacia la direcci�n de movimiento
        if (agent.velocity.magnitude > 0.1f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(agent.velocity.normalized); // Calcular rotaci�n objetivo
            transform.rotation = Quaternion.Slerp( // Rotar suavemente
                transform.rotation,
                targetRotation,
                Time.deltaTime * 5f // Velocidad de rotaci�n
            );
        }
    }

    #endregion

    #region M�todos P�blicos (para control externo)

    /// <summary>
    /// Hacer que el NPC se mueva a una posici�n espec�fica
    /// </summary>
    public void MoveToPosition(Vector3 position)
    {
        agent.SetDestination(position); // Establecer destino espec�fico
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
    /// Verificar si el NPC se est� moviendo actualmente
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

    #region Visualizaci�n de Debug

    // M�todo para dibujar gizmos en el editor cuando el objeto est� seleccionado
    private void OnDrawGizmosSelected()
    {
        // Dibujar radio de deambulaci�n
        Gizmos.color = Color.cyan; // Color cian
        Gizmos.DrawWireSphere(Application.isPlaying ? startPosition : transform.position, wanderRadius); // C�rculo del �rea

        // Dibujar ruta actual si se est� moviendo
        if (agent != null && agent.hasPath)
        {
            Gizmos.color = Color.yellow; // Color amarillo para la l�nea
            Gizmos.DrawLine(transform.position, agent.destination); // L�nea hacia destino

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