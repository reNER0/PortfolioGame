using Assets.Scripts.Network.Commands;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerInputController : MonoBehaviour
{
    public static NewInputSystem inputSystem { get; private set; }
    public static int previewTick { get; private set; }
    
    private bool jump;

    private TickRecorder TickRecorder;



    public float thinkInterval = 3f;

    private float nextThinkTime;

    private float botX, botY;

    private Player targetPlayer;


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

    private void FixedUpdate()
    {
        if (LaunchFlags.IsBot)
            BotUpdate();
        else
            ClientUpdate();
    }

    private void ClientUpdate()
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

        if (player.WeaponController.Weapon != null && player.WeaponController.Weapon.weaponObject.muzzle != null)
            aimDirection = lookPoint - player.WeaponController.Weapon.weaponObject.muzzle.position;

        if (player.WeaponController.Weapon != null && player.WeaponController.Weapon.GetType() == typeof(MeleeWeapon))
            aimDirection = lookPoint - player.transform.position;

        Tools.YawPitchFromDirection(aimDirection, out var yaw, out var pitch);

        bool fireInput = inputSystem.Inputs.Fire.IsPressed();
        bool aimInput = inputSystem.Inputs.Aim.IsPressed();

        //previewTick++;

        var input = new PlayerInputs(moveDirection.x, moveDirection.y, yaw, pitch, jump, fireInput, aimInput, NetworkTime.CurrentTick);

        jump = false;

        NetworkBus.OnCommandSendToServer?.Invoke(new InputCmd(input));

        // Client prediction
        if (NetworkRepository.Current.IsServer)
            return;


        // Sorting first player objects then other objects
        var allObjects = NetworkRepository.Current.NetworkObjectById;

        foreach (var predictable in allObjects.Select(x => x.Predictable))
            predictable.inputSeam = false;

        player.Input(input);

        // Apply Inputs, forces, etc
        foreach (var networkObject in allObjects)
        {
            // if input already applied - skip
            if (networkObject.Predictable.inputSeam)
                continue;

            networkObject.Predictable.Input(new PlayerInputs(0, 0, 0, 0, false, false, false, NetworkTime.CurrentTick));
        }


        if (NetworkSettings.MultiplayerType == MultiplayerType.Physics)
        {
            Physics.Simulate(Time.fixedDeltaTime);
            TickRecorder.RecordTick(NetworkTime.CurrentTick);
        }

        player.SaveCurrentState(NetworkTime.CurrentTick);
        NetworkBus.OnAllStatesSaved?.Invoke(NetworkTime.CurrentTick);
    }

    private void BotUpdate()
    {
        if (Time.time >= nextThinkTime)
        {
            Think();
            nextThinkTime = Time.time + thinkInterval;
        }

        var playerObject = NetworkRepository.Current.NetworkObjectById.FirstOrDefault(x => x.Id == NetworkRepository.Current.CurrentObjectId);

        if (playerObject == null)
            return;

        var player = (Player)playerObject.Predictable;

        if (player == null)
            return;

        if (player.GetHealth() == 0)
            return;

        var direction = player.transform.forward;

        bool shooting = targetPlayer != null && player.WeaponController.Weapon != null;

        if (shooting)
            direction = targetPlayer.transform.position - player.transform.position;

        Tools.YawPitchFromDirection(direction, out var yaw, out var pitch);


        var input = new PlayerInputs(botX, botY, yaw, pitch, jump, shooting, false, NetworkTime.CurrentTick);

        jump = false;

        NetworkBus.OnCommandSendToServer?.Invoke(new InputCmd(input));

        // Client prediction
        if (NetworkRepository.Current.IsServer)
            return;


        // Sorting first player objects then other objects
        var allObjects = NetworkRepository.Current.NetworkObjectById;

        foreach (var predictable in allObjects.Select(x => x.Predictable))
            predictable.inputSeam = false;

        player.Input(input);

        // Apply Inputs, forces, etc
        foreach (var networkObject in allObjects)
        {
            // if input already applied - skip
            if (networkObject.Predictable.inputSeam)
                continue;

            networkObject.Predictable.Input(new PlayerInputs(0, 0, 0, 0, false, false, false, NetworkTime.CurrentTick));
        }


        if (NetworkSettings.MultiplayerType == MultiplayerType.Physics)
        {
            Physics.Simulate(Time.fixedDeltaTime);
            TickRecorder.RecordTick(NetworkTime.CurrentTick);
        }

        player.SaveCurrentState(NetworkTime.CurrentTick);
        NetworkBus.OnAllStatesSaved?.Invoke(NetworkTime.CurrentTick);
    }

    void Think()
    {
        var maxDistance = 30f;

        var randomPos = new Vector3(Random.Range(-maxDistance, maxDistance), 0, Random.Range(-maxDistance, maxDistance));

        // Random go to weapon
        if (Random.Range(0, 1f) > 0.5f)
        {
            var weaponBoxes = FindObjectsOfType<WeaponBox>();
            randomPos = weaponBoxes[Random.Range(0, weaponBoxes.Length - 1)].transform.position;
        }



        var playerObject = NetworkRepository.Current.NetworkObjectById.FirstOrDefault(x => x.Id == NetworkRepository.Current.CurrentObjectId);
        if (playerObject == null)
            return;

        var player = (Player)playerObject.Predictable;
        if (player == null)
            return;

        // Random shoot
        if (Random.Range(0, 1f) > 0.5f)
        {
            var players = FindObjectsOfType<Player>().Where(x => x != player).Where(x => x.GetHealth() > 0).ToArray();
            targetPlayer = players[Random.Range(0, players.Length - 1)];
        }
        else
        {
            targetPlayer = null;
        }

            var direction = randomPos - player.transform.position;

        var maxSpeed = direction.magnitude / thinkInterval;

        var randomSpeed = Random.Range(0, player.MaxSpeed);

        direction.Normalize();
        direction *= Mathf.Min(randomSpeed, maxSpeed);
        direction /= player.MaxSpeed;

        botX = direction.x;
        botY = direction.z;

        Debug.LogError(randomPos);
    }


    private void OnJump(InputAction.CallbackContext context)
    {
        jump = true;
    }
}
