using System.Linq;
using Assets.Scripts.Network.Commands;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    public static NewInputSystem inputSystem { get; private set; }
    public static int previewTick { get; private set; }
    
    private bool jump;

    private TickRecorder TickRecorder;


    private void Awake()
    {
        TickRecorder = gameObject.AddComponent<TickRecorder>();

        inputSystem = new();
        inputSystem.Enable();
        inputSystem.Inputs.Jump.performed += OnJump;
    }

    private void OnDestroy()
    {
        inputSystem.Inputs.Jump.performed -= OnJump;
    }


    // Only on setup client
    public static void SetPreviewTick(int tick)
    {
        previewTick = tick;
    }


    private void Update()
    {
        var playerObject = NetworkRepository.Current.NetworkObjectById.FirstOrDefault(x => x.Id == NetworkRepository.Current.CurrentObjectId);

        if (playerObject == null)
            return;

        var player = (Player)playerObject.Predictable;

        if (player == null)
            return;

        Vector2 moveInput = inputSystem.Inputs.Move.ReadValue<Vector2>();
        var moveDirection = player.PlayerStateMachine.GetInputDirectionOverride(moveInput);

        var lookPoint = PlayerCamera.Instance.GetLookPoint();

        var aimDirection = player.transform.forward;

        if (player.WeaponController.Weapon != null)
            aimDirection = lookPoint - player.WeaponController.Weapon.weaponObject.muzzle.position;

        Tools.YawPitchFromDirection(aimDirection, out var yaw, out var pitch);

        bool fireInput = inputSystem.Inputs.Fire.IsPressed();
        bool aimInput = inputSystem.Inputs.Aim.IsPressed();

        while (previewTick < NetworkTime.CurrentTick)
        {
            previewTick++;

            var input = new PlayerInputs(moveDirection.x, moveDirection.y, yaw, pitch, jump, fireInput, aimInput, previewTick);

            jump = false;

            NetworkBus.OnCommandSendToServer?.Invoke(new InputCmd(input));

            // Client prediction
            if (NetworkRepository.Current.IsServer)
                return;

            player.Input(input);

            // Maybe extrapolate other players here

            if (NetworkSettings.MultiplayerType == MultiplayerType.Physics)
            {
                Physics.Simulate(Time.fixedDeltaTime);
                TickRecorder.RecordTick(previewTick);
            }

            player.SaveCurrentState(previewTick);
            NetworkBus.OnAllStatesSaved?.Invoke(previewTick);
        }
    }


    private void OnJump(InputAction.CallbackContext context)
    {
        jump = true;
    }
}
