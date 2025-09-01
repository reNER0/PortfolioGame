using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Network.Commands;
using UnityEngine;

[CreateAssetMenu(menuName = "State/GameState", order = 3)]
public class GameState : State
{
    public override void OnEnter()
    {
        Debug.Log("Starting game...");

        SetupCamera();
    }

    public override void OnUpdate()
    {

    }

    public override void OnExit()
    {
    }

    private void SetupCamera()
    {
        if (NetworkRepository.IsServer)
            return;

        var playerObject = NetworkRepository.NetworkObjectById.Values.FirstOrDefault(x => x.OwnerId == NetworkRepository.CurrentCliendId);

        if (playerObject == null)
        {
            Debug.LogError($"{nameof(GameState)} can`t setup camera: no player object!");
        }

        //PlayerCamera.SetTarget(playerObject.GameObject.transform);
    }
}
