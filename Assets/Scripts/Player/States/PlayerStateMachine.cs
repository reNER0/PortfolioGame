using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    private PlayerState currentState;


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

    public void OnInput(PlayerInputs playerInputs)
    {
        currentState?.OnInput(playerInputs);
    }
}
