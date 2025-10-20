using System.Linq;
using Assets.Scripts.Network.Commands;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    public NewInputSystem inputSystem { get; private set; }

    private bool jump;
    private static int previewTick;


    private void Awake()
    {
        inputSystem = new();
        inputSystem.Enable();
        inputSystem.Inputs.Jump.performed += OnJump;
    }

    private void OnDestroy()
    {
        inputSystem.Inputs.Jump.performed -= OnJump;
    }


    private void Update()
    {
        var playerObject = NetworkRepository.NetworkObjectById.FirstOrDefault(x => x.Id == NetworkRepository.CurrentObjectId);

        if (playerObject == null)
            return;

        var player = (Player)playerObject.Predictable;

        if (player == null)
            return;

        Vector2 moveInput = inputSystem.Inputs.Move.ReadValue<Vector2>();
        var moveDirection = player.PlayerStateMachine.GetInputDirectionOverride(moveInput);

        while (previewTick < NetworkTime.CurrentTick)
        {
            previewTick++;

            var input = new PlayerInputs(moveDirection.x, moveDirection.y, jump, previewTick);

            // Client prediction
            if (!NetworkRepository.IsServer)
            {
                player.Input(input);

                // Maybe extrapolate other players here

                if (NetworkSettings.MultiplayerType == MultiplayerType.Physics)
                {
                    Physics.Simulate(Time.fixedDeltaTime);
                }

                player.SaveCurrentState(previewTick);
                NetworkBus.OnAllStatesSaved?.Invoke(previewTick);
            }

            NetworkBus.OnCommandSendToServer?.Invoke(new InputCmd(input));

            jump = false;
        }
    }


    private void OnJump(InputAction.CallbackContext context)
    {
        jump = true;
    }
}
