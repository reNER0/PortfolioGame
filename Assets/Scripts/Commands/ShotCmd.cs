using System;
using Assets.Scripts.Commands;
using Assets.Scripts.Network.Commands;
using UnityEngine;

[Serializable]
public class ShotCmd : SerializableClass, ICommand
{
    [SerializeField]
    private int _tick;
    [SerializeField]
    private Vector3 _origin;
    [SerializeField]
    private Vector3 _direction;

    public ShotCmd(int tick, Vector3 origin, Vector3 direction)
    {
        _tick = tick;
        _origin = origin;
        _direction = direction;
    }

    public void Execute()
    {
        // If Server - do shot and send hit cmd to all clients
        // If Client - send to server

        if (NetworkRepository.Current.IsServer) 
        {
            var hitCollider = ShotProcessor.GetHit(_origin, _direction);

            if (hitCollider == null)
                return;

            var damagable = hitCollider.GetComponent<IDamagable>();

            if (damagable == null)
                return;

            //var hitCmd = new HitCmd(networkObject.Id);
            //NetworkBus.OnCommandSendToClients?.Invoke(hitCmd);

            damagable.Damage(70);

            return;
        }

        NetworkBus.OnCommandSendToServer?.Invoke(this);
    }

    public override string ToString()
    {
        return $"ShotCmd: tick={_tick}, origin={_origin}, direction={_direction}";
    }
}