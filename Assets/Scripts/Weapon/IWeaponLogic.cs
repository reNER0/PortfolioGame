namespace Assets.Scripts.Weapon
{
    public interface IWeaponLogic
    {
        void Attack(PlayerInputs playerInputs);
        bool NeedReload();
        void OnReload();
        void OnShowVisual();
    }
}
