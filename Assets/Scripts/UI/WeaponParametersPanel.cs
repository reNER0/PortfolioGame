using TMPro;
using UnityEngine;

public class WeaponParametersPanel : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI currentAmmoText;
    [SerializeField]
    private TextMeshProUGUI maxAmmoText;


    private GunWeapon currentGunWeapon;


    private void Start()
    {
        GameBus.OnLocalWeaponPickup += OnWeaponPickup;

        Show(false);
    }

    private void OnDestroy()
    {
        GameBus.OnLocalWeaponPickup -= OnWeaponPickup;
    }

    private void Show(bool show)
    {
        gameObject.SetActive(show);
    }

    private void OnWeaponPickup(Weapon weapon)
    {
        UnsubscribeIfNeeded();

        if (weapon is GunWeapon gunWeapon)
        {
            currentGunWeapon = (GunWeapon)weapon;
            var gunWeaponModel = (GunModel)weapon.weaponModel;

            maxAmmoText.text = gunWeaponModel.ammoCapacity.ToString();
            OnAmmo(gunWeapon.CurrentAmmo);
            gunWeapon.OnAmmo += OnAmmo;

            Show(true);
            return;
        }

        Show(false);
    }

    private void UnsubscribeIfNeeded() 
    {
        if (currentGunWeapon != null)
        {
            currentGunWeapon.OnAmmo -= OnAmmo;
            currentGunWeapon = null;
        }
    }

    private void OnAmmo(int ammo)
    {
        currentAmmoText.text = ammo.ToString();
    }
}
