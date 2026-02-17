using Assets.Scripts.Weapon;

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
    public int currentAmmo;

    public GunWeapon(GunModel weaponModel, WeaponPrefab weaponPrefab, Player player) : base(weaponModel, weaponPrefab)
    {
        this.currentAmmo = weaponModel.ammoCapacity;
        weaponLogic = new GunWeaponLogic(this, weaponModel, player);
    }
}

public class MeleeWeapon : Weapon
{
    public MeleeWeapon(MeleeModel weaponModel, WeaponPrefab weaponPrefab, Player player) : base(weaponModel, weaponPrefab) 
    {
        weaponLogic = new MeleeWeaponLogic(player);
    }
}
