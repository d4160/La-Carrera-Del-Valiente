// Core/Entities/Entity.cs
using UnityEngine;
using System;

/// <summary>
/// Clase base para todas las entidades interactivas en el juego.
/// Proporciona una identidad �nica y un ciclo de vida b�sico.
/// </summary>
public abstract class Entity : MonoBehaviour, IEquatable<Entity>
{
    [Header("Entity Core")]
    // Genera un ID persistente en el editor y �nico en tiempo de ejecuci�n.
    // Se recomienda usar un paquete como "Odin Inspector" o una soluci�n custom
    // para generar y visualizar GUIDs de forma m�s robusta en el Inspector.
    // Esta implementaci�n simple funciona pero puede regenerarse si se copia el componente.
    [SerializeField, HideInInspector] private string _guid = System.Guid.NewGuid().ToString();
    public string Guid => _guid;

    // Eventos b�sicos del ciclo de vida
    public event Action<Entity> OnInitialized;
    public event Action<Entity> OnWillBeDestroyed; // Antes de que se destruya el GameObject

    protected virtual void Awake()
    {
        // Podr�a haber l�gica com�n aqu�, pero mantenerla m�nima.
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
    /// L�gica de inicializaci�n espec�fica de la entidad.
    /// Llamado desde Start() o manualmente si se requiere un orden espec�fico.
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