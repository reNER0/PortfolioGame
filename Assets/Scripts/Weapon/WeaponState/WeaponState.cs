using UnityEngine;

public abstract class WeaponState : State
{
    protected WeaponController _weaponController;

    public WeaponState(WeaponController weaponController)
    {
        _weaponController = weaponController;
    }

    public abstract void OnInput(PlayerInputs playerInputs);
}

