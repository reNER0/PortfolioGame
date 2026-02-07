using UnityEngine;

public class WeaponStateMachine : MonoBehaviour
{
    public WeaponState currentState { get; private set; }


    public void ChangeState(WeaponState state)
    {
        currentState?.OnExit();

        currentState = state;

        currentState?.OnEnter();
    }

    public void OnInput(PlayerInputs playerInputs)
    {
        currentState?.OnInput(playerInputs);
    }
}
