using System;
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

        GameBus.OnPredictableHit?.Invoke(NetworkRepository.Current.NetworkObjectById[_hitObjectId].Predictable,_damage);
    }
}