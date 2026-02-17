using DG.Tweening;
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

    private bool isAiming;

    private Tween _layerTween;

    private bool _lastFire;


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

        Aim(playerInputs.Aim);

        if (isReloading)
            return;

        if (isPreparing)
            return;

        if (Weapon.weaponLogic.NeedReload())
        {
            // TODO : make automatic reload
        }

        bool fireHeld = playerInputs.Fire;
        bool fireDown = fireHeld && !_lastFire;
        bool fireUp = !fireHeld && _lastFire;


        // --- VISUAL (local prediction) ---
        if (fireDown)
            OnStartShooting();
        if (fireUp)
            OnStopShooting();

        Weapon.weaponLogic.Attack(playerInputs);

        _lastFire = fireHeld;
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
        isAiming = aim;
        Animator.SetBool("IsAiming", aim);
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

        if (!isAiming && !isPreparing && !isReloading && !Weapon.weaponModel.isTwoHanded)
        {
            SetLeftHandIK(0);
            SetUpperBodyWeight(0);
            return;
        }

        SetUpperBodyWeight(1);


        if (Weapon.weaponModel.isTwoHanded)
        {
            SetLeftHandIK(1);
            return;
        }

        if (isAiming) 
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


    private void OnStartShooting()
    {
        OnStopShooting();

        ShowVisualTrail();

        _shootTween = DOVirtual.DelayedCall(
            Weapon.weaponModel.fireRate,
            ShowVisualTrail
        )
        .SetLoops(-1, LoopType.Restart)
        .SetUpdate(UpdateType.Normal);
    }

    private void OnStopShooting()
    {
        _shootTween?.Kill();
    }


    private void ShowVisualTrail()
    {
        var camera = Camera.main;

        if (!Physics.Raycast(weapon.weaponObject.muzzle.position, direction, out var hit, weapon.weaponModel.range))
            return;

        GameBus.OnBulletFX?.Invoke(new BulletFX
        {
            StartPosition = weapon.weaponObject.muzzle.position,
            EndPosition = hit.point
        });
    }
}