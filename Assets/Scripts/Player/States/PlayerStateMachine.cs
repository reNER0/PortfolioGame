using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    public PlayerState currentState { get; private set; }


    public void ChangeState(PlayerState state)
    {
        currentState?.OnExit();

        currentState = state;

        currentState?.OnEnter();
    }

    public void Update()
    {
        currentState?.OnUpdate();
    }

    private void OnAnimatorIK(int layerIndex)
    {
        currentState?.OnAnimatorIK(layerIndex);
    }

    public void OnCollisionEnter(Collision collision)
    {
        currentState?.OnCollisionEnter(collision);
    }

    public void OnInput(PlayerInputs playerInputs)
    {
        currentState?.OnInput(playerInputs);
    }

    public virtual Vector2 GetInputDirectionOverride(Vector2 input)
    {
        return currentState?.GetInputDirectionOverride(input) ?? Vector2.zero;
    }
}
