using UnityEngine;

[System.Serializable]
public class YellowState : TrafficLightState
{
    public override void ChangeToNextState()
    {
        (_machineRef as TrafficLightStateMachine).ChangeToRedOrGreenState();
    }
}
