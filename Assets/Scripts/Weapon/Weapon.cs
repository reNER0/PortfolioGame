using Assets.Scripts.Weapon;
using System;

public abstract class Weapon
{
    public IWeaponLogic weaponLogic;
    public WeaponModel weaponModel;
    public WeaponPrefab weaponObject;

    public Weapon(WeaponModel weaponModel, WeaponPrefab weaponPrefab)
    {
        this.weaponModel = weaponModel;
        this.weaponObject = weaponPrefab;
    }
}

public class GunWeapon : Weapon
{
    public event Action<int> OnAmmo;
    public int CurrentAmmo { get; private set; }

    public GunWeapon(GunModel weaponModel, WeaponPrefab weaponPrefab, Player player) : base(weaponModel, weaponPrefab)
    {
        this.CurrentAmmo = weaponModel.ammoCapacity;
        weaponLogic = new GunWeaponLogic(this, weaponModel, player);
    }

    public bool CanShoot() 
    {
        return CurrentAmmo > 0;
    }

    public void WasteAmmo() 
    {
        CurrentAmmo--;
        OnAmmo?.Invoke(CurrentAmmo);
    }

    public void Reload()
    {
        CurrentAmmo = ((GunModel)weaponModel).ammoCapacity;
        OnAmmo?.Invoke(CurrentAmmo);
    }
}

public class MeleeWeapon : Weapon
{
    public MeleeWeapon(MeleeModel weaponModel, WeaponPrefab weaponPrefab, Player player) : base(weaponModel, weaponPrefab) 
    {
        weaponLogic = new MeleeWeaponLogic(player);
    }
}
