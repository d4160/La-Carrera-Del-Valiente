// Core/Entities/Entity.cs
using UnityEngine;
using System;

/// <summary>
/// Clase base para todas las entidades interactivas en el juego.
/// Proporciona una identidad única y un ciclo de vida básico.
/// </summary>
public abstract class Entity : MonoBehaviour, IEquatable<Entity>
{
    [Header("Entity Core")]
    // Genera un ID persistente en el editor y único en tiempo de ejecución.
    // Se recomienda usar un paquete como "Odin Inspector" o una solución custom
    // para generar y visualizar GUIDs de forma más robusta en el Inspector.
    // Esta implementación simple funciona pero puede regenerarse si se copia el componente.
    [SerializeField, HideInInspector] private string _guid = System.Guid.NewGuid().ToString();
    public string Guid => _guid;

    // Eventos básicos del ciclo de vida
    public event Action<Entity> OnInitialized;
    public event Action<Entity> OnWillBeDestroyed; // Antes de que se destruya el GameObject

    protected virtual void Awake()
    {
        // Podría haber lógica común aquí, pero mantenerla mínima.
    }

    protected virtual void Start()
    {
        InitializeEntity();
    }

    protected virtual void OnDestroy()
    {
        OnWillBeDestroyed?.Invoke(this);
    }

    /// <summary>
    /// Lógica de inicialización específica de la entidad.
    /// Llamado desde Start() o manualmente si se requiere un orden específico.
    /// </summary>
    public virtual void InitializeEntity()
    {
        // Debug.Log($"Entity Initialized: {gameObject.name} (GUID: {Guid})");
        OnInitialized?.Invoke(this);
    }

    #region Equality (Basado en GUID)
    public bool Equals(Entity other)
    {
        if (other == null) return false;
        return this.Guid == other.Guid;
    }

    public override bool Equals(object obj)
    {
        return Equals(obj as Entity);
    }

    public override int GetHashCode()
    {
        return Guid.GetHashCode();
    }

    public static bool operator ==(Entity left, Entity right)
    {
        if (ReferenceEquals(left, null)) return ReferenceEquals(right, null);
        return left.Equals(right);
    }

    public static bool operator !=(Entity left, Entity right)
    {
        return !(left == right);
    }
    #endregion

    // --- Metodos comunes potenciales ---
    // public virtual void TakeDamage(DamageInfo damageInfo) { }
    // public virtual void Interact(Entity interactor) { }
    // public virtual void Despawn() { Destroy(gameObject); } // O usar pooling
}