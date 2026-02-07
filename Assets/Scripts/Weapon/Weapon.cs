using UnityEngine;

public class Weapon
{
    public WeaponModel weaponModel;
    public WeaponPrefab weaponObject;
    public int ammoCount;

    public Weapon(WeaponModel weaponModel, Transform socket)
    {
        this.weaponModel = weaponModel;

        weaponObject = GameObject.Instantiate(weaponModel.prefab, socket);
    }


    public void OnReload() 
    {
        if (weaponModel.reloadMode == ReloadMode.Magazine)
        {
            ammoCount = weaponModel.ammoCapacity;
            return;
        }

        ammoCount++;
    }

    public bool NeedReload()
    {
        return ammoCount < weaponModel.ammoCapacity;
    }
}
