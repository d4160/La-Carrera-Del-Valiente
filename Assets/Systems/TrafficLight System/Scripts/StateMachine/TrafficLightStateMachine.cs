using UnityEngine;

public class TrafficLightStateMachine : StateMachine
{
    public TrafficLight entityRef;

    [Header("States")]
    public GreenState greenState;
    public YellowState yellowState;
    public RedState redState;

    private TrafficLightState _redOrGreenState;

    private void Start()
    {
        greenState.SetMachine(this);
        yellowState.SetMachine(this);
        redState.SetMachine(this);

        ChangeToGreenState();
    }

    public void ChangeToYellowState()
    {
        ChangeToState(yellowState);
    }

    public void ChangeToGreenState()
    {
        _redOrGreenState = greenState;
        ChangeToState(greenState);
    }

    public void ChangeToRedState()
    {
        _redOrGreenState = redState;
        ChangeToState(redState);
    }

    public void ChangeToRedOrGreenState()
    {
        if (_redOrGreenState.GetType() == typeof(GreenState))
        {
            ChangeToRedState();
        }
        else
        {
            ChangeToGreenState();
        }        
    }
}
