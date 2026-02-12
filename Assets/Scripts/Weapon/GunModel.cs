using UnityEngine;

[CreateAssetMenu(menuName = "Weapon/NewGun", order = 1)]
public class GunModel : WeaponModel
{
    public int bulletsPerShot = 1;
    public float spread = 10;
    public float fireRate = 10;
    public int ammoCapacity = 30;
    public float reloadTime;
    public FireMode fireMode;
    public ReloadMode reloadMode;
}