using UnityEngine;

[System.Serializable]
public class RedState : TrafficLightState
{
    public override void ChangeToNextState()
    {
        (_machineRef as TrafficLightStateMachine).ChangeToYellowState();
    }
}
