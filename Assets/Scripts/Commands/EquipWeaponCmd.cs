using System;
using System.Linq;
using Assets.Scripts.Network.Commands;
using UnityEngine;

[Serializable]
public class EquipWeaponCmd : SerializableClass, ICommand
{
    [SerializeField]
    private int _playerId;
    [SerializeField]
    private string _weaponModelName;

    public EquipWeaponCmd(int playerId, string weaponModelName)
    {
        _playerId = playerId;
        _weaponModelName = weaponModelName;
    }

    public void Execute()
    {
        var player = (Player)NetworkRepository.Current.NetworkObjectById.First(x => x.Id == _playerId).Predictable;

        if (NetworkRepository.Current.IsServer)
        {
            NetworkBus.OnCommandSendToClients(this);
        }

        WeaponModel weaponModel = Resources.Load<WeaponModel>($"Weapons/{_weaponModelName}");
        player.WeaponController.PickupWeapon(weaponModel);
    }
}