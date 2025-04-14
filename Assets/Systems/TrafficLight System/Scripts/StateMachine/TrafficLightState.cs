using UnityEngine;

[System.Serializable]
public class TrafficLightState : State
{
    public float duration;
    public Color color;

    private float _elapsedTime;

    public override void OnEnter()
    {
        _elapsedTime = 0;
        (_machineRef as TrafficLightStateMachine).entityRef.spriteRendererRef.color = color;
    }

    public override void Update()
    {
        _elapsedTime += Time.deltaTime;

        if (_elapsedTime > duration)
        {
            ChangeToNextState();
        }
    }

    public virtual void ChangeToNextState()
    { }
}
