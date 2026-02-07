using UnityEngine;

public enum FireMode
{
    Automatic,
    SemiAuto,
    Manual
}

public enum ReloadMode
{
    Magazine,
    Bullet
}


[CreateAssetMenu(menuName = "Weapon/NewWeapon", order = 1)]
public class WeaponModel : ScriptableObject
{
    public WeaponPrefab prefab;

    public int damage = 10;
    public int fireRate = 10;
    public int bulletsPerShot = 1;
    public float spread = 10;
    public int ammoCapacity = 30;
    public float reloadTime;
    public FireMode fireMode;
    public ReloadMode reloadMode;

    public bool isMelee;
}
