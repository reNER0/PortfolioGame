using Assets.Scripts.Network.Commands;
using DG.Tweening;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class WeaponController : MonoBehaviour
{
    public AudioClip AudioClip;
    public AudioSource AudioSource { get; private set; }
    public PlayerAnimationEvents PlayerAnimationEvents { get; private set; }
    public Animator Animator { get; private set; }

    public Transform WeaponSocket;

    public Weapon Weapon;

    private bool isReloading;

    private bool isPreparing;

    public bool IsAiming { get; private set; }

    private Tween _layerTween;

    //private bool _lastFire;

    private Tween _shootTween;



    private void Awake()
    {
        Animator = GetComponentInChildren<Animator>();

        PlayerAnimationEvents = Animator.AddComponent<PlayerAnimationEvents>();

        AudioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
    {
        if (NetworkRepository.Current.IsCurrentClientOwnerOfObject(GetComponent<Predictable>()))
            PlayerInputController.inputSystem.Inputs.Reload.performed += OnReload;

        PlayerAnimationEvents.OnReloadAnimationFinished += OnReloadAnimationFinished;
        PlayerAnimationEvents.OnPrepareAnimationFinished += OnPrepareAnimationFinished;
    }
    private void OnDestroy()
    {
        PlayerInputController.inputSystem.Inputs.Reload.performed -= OnReload;

        PlayerAnimationEvents.OnReloadAnimationFinished -= OnReloadAnimationFinished;
        PlayerAnimationEvents.OnPrepareAnimationFinished -= OnPrepareAnimationFinished;
    }

    public void PickupWeapon(WeaponModel weaponModel)
    {
        if (Weapon != null)
            Destroy(Weapon.weaponObject.gameObject);

        var player = GetComponent<Player>();
        Weapon = WeaponFactory.CreateWeapon(player, weaponModel, WeaponSocket);
    }

    private void OnReload(InputAction.CallbackContext context)
    {
        OnReload();
    }

    // same as FixedUpdate
    public void Input(PlayerInputs playerInputs)
    {
        if (Weapon == null)
            return;

        //Aim(playerInputs.Aim);

        if (isReloading)
            return;

        if (isPreparing)
            return;

        if (Weapon.weaponLogic.NeedReload())
        {
            // TODO : make automatic reload
        }

        /*
        bool fireHeld = playerInputs.Fire;
        bool fireDown = fireHeld && !_lastFire;
        bool fireUp = !fireHeld && _lastFire;
        */

        Weapon.weaponLogic.Attack(playerInputs);

        //_lastFire = fireHeld;
    }


    public void OnReloadAnimationStarted()
    {
        Animator.SetBool("IsReloading", true);
    }
    public void OnReloadAnimationFinished()
    {
        Animator.SetBool("IsReloading", false);
        OnReloadFinished();
    }

    private void Aim(bool aim)
    {
        var animationName = "IsAiming";

        IsAiming = aim;
        Animator.SetBool(animationName, aim);

        if (!NetworkRepository.Current.IsServer)
            return;

        var playerObjectId = NetworkRepository.Current.NetworkObjectById.First(x => x.Predictable == GetComponent<Predictable>()).Id;

        var attackAnimationCmd = new SetPlayerAnimatorBoolCmd(playerObjectId, animationName, aim);

        var networkClient = NetworkRepository.Current.ConnectedClients.FirstOrDefault(x => x.ClientObjectId == playerObjectId);

        if (networkClient == null)
            return;

        NetworkBus.OnCommandSendToClientsExcept(attackAnimationCmd, networkClient);
    }

    private void OnReload()
    {
        isReloading = true;
        OnReloadAnimationStarted();
    }

    private void OnReloadFinished()
    {
        isReloading = false;
        Weapon.weaponLogic.OnReload();
        OnPrepare();
    }

    private void OnPrepare()
    {
        isPreparing = true;
        OnPrepareAnimationStarted();
    }

    public void OnPrepareAnimationStarted()
    {
        Animator.SetBool("IsPreparing", true);
    }
    public void OnPrepareAnimationFinished()
    {
        Animator.SetBool("IsPreparing", false);
        OnPrepareFinished();
    }

    private void OnPrepareFinished()
    {
        isPreparing = false;
    }


    private void OnAnimatorIK(int layerIndex)
    {
        if (layerIndex != 1)
            return;

        if (Weapon == null)
        {
            SetLeftHandIK(0);
            SetUpperBodyWeight(0);
            return;
        }

        if (!IsAiming && !isPreparing && !isReloading && !Weapon.weaponModel.isTwoHanded)
        {
            SetLeftHandIK(0);
            SetUpperBodyWeight(0);
            return;
        }

        SetUpperBodyWeight(1, 0);


        if (Weapon.weaponModel.isTwoHanded)
        {
            SetLeftHandIK(1);
            return;
        }

        if (IsAiming) 
        {
            SetLeftHandIK(1);
            return;
        }

        SetLeftHandIK(0);
    }

    private void SetLeftHandIK(float ikWeight)
    {
        var avatarIKGoal = AvatarIKGoal.LeftHand;

        Animator.SetIKPositionWeight(avatarIKGoal, ikWeight);
        Animator.SetIKRotationWeight(avatarIKGoal, ikWeight);

        if (Weapon == null)
            return;

        Animator.SetIKPosition(avatarIKGoal, Weapon.weaponObject.leftHandGrip.transform.position);
        Animator.SetIKRotation(avatarIKGoal, Weapon.weaponObject.leftHandGrip.transform.rotation);
    }

    private void SetUpperBodyWeight(float targetWeight, float duration = 0.25f)
    {
        _layerTween?.Kill();

        float current = Animator.GetLayerWeight(1);

        _layerTween = DOTween.To(
            () => current,
            x =>
            {
                current = x;
                Animator.SetLayerWeight(1, current);
            },
            targetWeight,
            duration
        )
        .SetEase(Ease.OutSine);
    }


    public void OnStartShooting()
    {
        OnStopShooting();

        Aim(true);

        Weapon.weaponLogic.OnShowVisual();

        _shootTween = DOVirtual.DelayedCall(
                Weapon.weaponModel.fireRate,
                Weapon.weaponLogic.OnShowVisual)
            .SetLoops(-1, LoopType.Restart)
            .SetUpdate(UpdateType.Normal);
    }

    public void OnStopShooting()
    {
        Aim(false);
        _shootTween?.Kill();
    }
}