using System;
using System.Linq;
using Assets.Scripts.Network.Commands;
using UnityEngine;

[Serializable]
public class HitCmd : SerializableClass, ICommand
{
    [SerializeField]
    private int _hitObjectId;
    [SerializeField]
    private int _damage;

    public HitCmd(int hitObjectId, int damage)
    {
        _hitObjectId = hitObjectId;
        _damage = damage;
    }

    public void Execute()
    {
        bool youWasHit = NetworkRepository.Current.CurrentObjectId == _hitObjectId;

        if (youWasHit)
        {
            GameBus.OnBadEffect?.Invoke();
            return;
        }

        var networkObject = NetworkRepository.Current.NetworkObjectById.FirstOrDefault(x => x.Id == _hitObjectId);

        if (networkObject == null)
            return;

        GameBus.OnPredictableHit?.Invoke(networkObject.Predictable, _damage);
    }

    public override string ToString()
    {
        return $"HitCmd: hitObjectId={_hitObjectId}, damage={_damage}";
    }
}