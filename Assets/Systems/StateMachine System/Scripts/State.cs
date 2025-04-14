using UnityEngine;

[System.Serializable] // Permitimos que la informacion de esta clase se puede guardar
public class State
{
    /// <summary>
    /// Referencia a su maquina de estados
    /// </summary>
    protected StateMachine _machineRef;

    /// <summary>
    /// Invocado al Entrar al estado
    /// </summary>
    public virtual void OnEnter()
    { }

    /// <summary>
    /// Actualizar en cada frame
    /// Invocado por StateMachine
    /// </summary>
    public virtual void Update()
    {}

    /// <summary>
    /// Invocado al Salir al estado
    /// </summary>
    public virtual void OnExit()
    { }

    /// <summary>
    /// Pone el valor adecuado de _machineRef 
    /// </summary>
    public void SetMachine(StateMachine machineRef)
    {
        this._machineRef = machineRef;
    }
}

