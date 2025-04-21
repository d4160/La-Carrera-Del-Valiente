using UnityEngine;

[DisallowMultipleComponent]
public class StateMachine : MonoBehaviour
{
    protected State _currentState; // camelCase

    void Update()
    {
        _currentState.Update();
    }

    public void ChangeToState(State newState)
    {
        if (_currentState != null)
        {
            _currentState.OnExit();
        }

        if (newState != null)
        {
            newState.OnEnter();
        }

        _currentState = newState;
    }
}
