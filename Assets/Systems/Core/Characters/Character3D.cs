// Core/Characters/Character.cs
using UnityEngine;

/// <summary>
/// Clase base para entidades que pueden ser controladas por un jugador o IA.
/// Contiene referencias a componentes comunes de personajes.
/// </summary>
[RequireComponent(typeof(Rigidbody))] // O Rigidbody2D
// Añadir otros RequireComponent comunes (StateMachine, etc.) si son *absolutamente* universales
public abstract class Character3D<TData> : Entity where TData : CharacterDataSO
{
    [Header("Character Core Data")]
    [SerializeField] protected TData _characterData;
    public TData Data => _characterData;

    [Header("Common Component References")]
    public Rigidbody Rb { get; protected set; } // O Rigidbody2D Rb2D
    public Collider Col { get; protected set; } // O Collider2D Col2D
    public StateMachine StateMachine { get; protected set; }
    //public IAgentController Controller { get; protected set; }
    //public IAnimatorAdapter AnimatorAdapter { get; protected set; }
    //public IEffectPlayer EffectPlayer { get; protected set; }

    // --- Runtime Stats (Ejemplos) ---
    public virtual float CurrentHealth { get; protected set; }
    public virtual bool IsDead => CurrentHealth <= 0;

    protected override void Awake()
    {
        base.Awake();
        FindCommonComponents();
        InitializeStats();
    }

    /// <summary>
    /// Busca y asigna referencias a componentes comunes. Sobrescribir si es necesario.
    /// </summary>
    protected virtual void FindCommonComponents()
    {
        Rb = GetComponent<Rigidbody>(); // O Rigidbody2D
        Col = GetComponent<Collider>(); // O Collider2D
        StateMachine = GetComponentInChildren<StateMachine>(); // Permite anidamiento
        //Controller = GetComponentInChildren<IAgentController>();
        //AnimatorAdapter = GetComponentInChildren<IAnimatorAdapter>(); // Busca implementación
        //EffectPlayer = GetComponentInChildren<IEffectPlayer>(); // Busca implementación

        // --- Validaciones ---
        //if (Controller == null) DebugLogWarning("IAgentController implementation not found.");
        //if (StateMachine == null) DebugLogWarning("StateMachine component not found.");
        //if (AnimatorAdapter == null) DebugLogWarning("IAnimatorAdapter implementation not found.");
        // Añadir más validaciones según la criticidad
    }

    /// <summary>
    /// Inicializa los stats runtime basados en CharacterDataSO.
    /// </summary>
    protected virtual void InitializeStats()
    {
        if (_characterData != null)
        {
            CurrentHealth = _characterData.MaxHealth;
        }
        else
        {
            DebugLogError("CharacterDataSO is not assigned!");
        }
    }

    protected virtual void Update()
    {
        if (IsDead) return;
        //StateMachine?.LogicUpdate();
        //AnimatorAdapter?.Tick(Time.deltaTime); // Para lógica interna del Adapter si la tiene
    }

    protected virtual void FixedUpdate()
    {
        if (IsDead) return;
        //StateMachine?.PhysicsUpdate();
    }

    public override void InitializeEntity()
    {
        base.InitializeEntity();
        // Inicializar StateMachine y Actions después de que todo esté listo
        InitializeStateMachine();
        InitializeActions();
    }

    /// <summary>
    /// Inicializa la State Machine con el estado inicial apropiado.
    /// Debe ser implementado por clases derivadas (e.g., Platformer2DCharacter).
    /// </summary>
    protected abstract void InitializeStateMachine();

    /// <summary>
    /// Busca e inicializa los componentes de Acción asociados a este personaje.
    /// Puede ser sobrescrito para inicializar acciones específicas.
    /// </summary>
    protected virtual void InitializeActions()
    {
        //var actions = GetComponentsInChildren<IAction>(); // Busca todas las acciones
        //foreach (var action in actions)
        //{
        //    action.Setup(this);
        //}
    }

    // --- Logging Helpers ---
    protected void DebugLogWarning(string message) => Debug.LogWarning($"{GetType().Name} ({gameObject.name}): {message}", this);
    protected void DebugLogError(string message) => Debug.LogError($"{GetType().Name} ({gameObject.name}): {message}", this);
}

// --- Ejemplo de especialización (iría en su propio sistema/carpeta) ---
// Systems/Platformer2D/Platformer2DCharacter.cs
// using UnityEngine;
// public class Platformer2DCharacter : Character<PlatformerCharacterDataSO>
// {
//     // Referencias a estados *específicos* de Platformer
//     public PlatformerIdleState IdleState { get; private set; }
//     public PlatformerMoveState MoveState { get; private set; }
//     // ... otros estados
//
//     // Referencias a acciones *específicas* (opcional, podrían ser accedidas genéricamente)
//     public GroundMovementAction GroundMovement { get; private set; }
//     public JumpAction Jump { get; private set; }
//
//     protected override void FindCommonComponents() {
//         base.FindCommonComponents(); // Llama a la base
//         // Aquí podrías buscar componentes específicos 2D como Rigidbody2D
//         // Rb2D = GetComponent<Rigidbody2D>();
//         // Col2D = GetComponent<Collider2D>();
//     }
//
//     protected override void InitializeStateMachine() {
//          if (StateMachine == null) return;
//          // Crear instancias de estados específicos
//          IdleState = new PlatformerIdleState(StateMachine, this);
//          MoveState = new PlatformerMoveState(StateMachine, this);
//          // ... crear otros estados
//          StateMachine.Initialize(IdleState, this); // Iniciar con Idle
//     }
//
//     protected override void InitializeActions() {
//         base.InitializeActions(); // Inicializa todas las IAction
//         // Obtener referencias específicas si se necesitan frecuentemente
//         GroundMovement = GetComponentInChildren<GroundMovementAction>();
//         Jump = GetComponentInChildren<JumpAction>();
//     }
// }

// Systems/Spaceship/SpaceshipCharacter.cs - Similar estructura