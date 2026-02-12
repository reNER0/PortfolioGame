using System;
using UnityEngine;

public static class WeaponFactory
{
    public static Weapon CreateWeapon(Player player, WeaponModel weaponModel, Transform weaponSocket)
    {
        var weaponObject = GameObject.Instantiate(weaponModel.prefab, weaponSocket);

        return weaponModel switch
        {
            GunModel gun => new GunWeapon(gun, weaponObject, player),
            MeleeModel mel => new MeleeWeapon(mel, weaponObject, player),
            _ => throw new ArgumentOutOfRangeException(nameof(weaponModel), weaponModel.GetType(), null)
        };
    }
}