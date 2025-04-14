using UnityEngine;

[System.Serializable]
public class GreenState : TrafficLightState
{
    public override void OnEnter()
    {
        base.OnEnter();
        (_machineRef as TrafficLightStateMachine).entityRef.spriteRendererRef.color = Color.green;
    }

    public override void ChangeToNextState()
    {
        (_machineRef as TrafficLightStateMachine).ChangeToYellowState();
    }
}
