using System;
using System.Linq;
using Assets.Scripts.Network.Commands;
using UnityEditor.Rendering;
using UnityEngine;

public class PlayerStandingState : PlayerState
{
    private DateTime creationTime = DateTime.Now;
    private float sleepTime = 1 / 10f;

    protected float lastDistance;
    protected bool isGrounded = false;
    private bool firstGroundTouch = false;
    private Vector3 currentVelocity;

    private bool isAiming;

    protected bool isJumped = true;

    private Rigidbody standingRigidbody;
    private RaycastHit hit;


    public PlayerStandingState(Player player) : base(player) { }


    public override void OnEnter()
    {
        lastDistance = _player.SpringDistance;
    }

    public override void OnUpdate()
    {
        var velocity = _player.Rigidbody.velocity;

        if (standingRigidbody != null)
            velocity -= standingRigidbody.velocity;

        Vector3 localVelocity = _player.transform.InverseTransformDirection(velocity);

        _player.Animator.SetFloat("VelocityX", localVelocity.x / _player.MaxSpeed);
        _player.Animator.SetFloat("VelocityY", localVelocity.z / _player.MaxSpeed);
        _player.Animator.SetBool("IsGrounded", isGrounded);
    }

    public override void OnAnimatorIK(int layer)
    {
        if (!_player.UseIK)
            return;

        SetLegIK(AvatarIKGoal.LeftFoot);
        SetLegIK(AvatarIKGoal.RightFoot);
        //SetHandIK(AvatarIKGoal.LeftHand);
        //SetHandIK(AvatarIKGoal.RightHand);

        //_player.Animator.SetLayerWeight(1, _player.WeaponController.IsUsingRightHand() ? 1 : 0);
    }

    private void SetHandIK(AvatarIKGoal avatarIKGoal)
    {
        float ikWeight = 0;

        switch (avatarIKGoal)
        {
            case AvatarIKGoal.LeftHand:
                ikWeight = _player.WeaponController.IsUsingLeftHand() ? 1 : 0;

                _player.Animator.SetIKPosition(avatarIKGoal, _player.WeaponController.Weapon.weaponObject.leftHandGrip.transform.position);
                _player.Animator.SetIKRotation(avatarIKGoal, _player.WeaponController.Weapon.weaponObject.leftHandGrip.transform.rotation);
                break;

            case AvatarIKGoal.RightHand:
                ikWeight = _player.WeaponController.IsUsingRightHand() ? 1 : 0;

                _player.Animator.SetIKPosition(avatarIKGoal, _player.WeaponController.Weapon.weaponObject.rightHandGrip.transform.position);
                _player.Animator.SetIKRotation(avatarIKGoal, _player.WeaponController.Weapon.weaponObject.rightHandGrip.transform.rotation);
                break;
        }

        _player.Animator.SetIKPositionWeight(avatarIKGoal, ikWeight);
        _player.Animator.SetIKRotationWeight(avatarIKGoal, ikWeight);
    }

    private void SetLegIK(AvatarIKGoal avatarIKGoal)
    {
        float ikWeight = 0;

        switch (avatarIKGoal)
        {
            case AvatarIKGoal.LeftFoot:
                ikWeight = _player.Animator.GetFloat("LeftFootIKWeight");
                break;
            case AvatarIKGoal.RightFoot:
                ikWeight = _player.Animator.GetFloat("RightFootIKWeight");
                break;
        }

        _player.Animator.SetIKPositionWeight(avatarIKGoal, ikWeight);
        _player.Animator.SetIKRotationWeight(avatarIKGoal, ikWeight);

        var startingPoint = _player.Animator.GetIKPosition(avatarIKGoal);
        startingPoint.y = _player.transform.position.y + 1;

        RaycastHit hit;

        if (Physics.Raycast(startingPoint, Vector3.down, out hit, _player.SpringDistance, _player.WalkableLayerMask))
        {
            _player.Animator.SetIKPosition(avatarIKGoal, hit.point + hit.normal * _player.LegsIKOffset);
            _player.Animator.SetIKRotation(avatarIKGoal, Quaternion.LookRotation(_player.transform.forward, hit.normal));
        }
    }

    public override void OnInput(PlayerInputs playerInputs)
    {
        //if (!isGrounded)
        //    ApplyAdditiveGravity();

        if (!IsSleepTimeElapsed())
            return;

        if (_player.Rigidbody.velocity.y <= 0)
            isJumped = false;

        if (!isJumped)
            ApplySpringForce();
    }

    public override void OnExit()
    {

    }


    private bool IsSleepTimeElapsed()
    {
        return (DateTime.Now - creationTime).TotalSeconds > sleepTime;
    }


    private void ApplySpringForce()
    {
        if (Physics.Raycast(_player.transform.position + _player.transform.up, -_player.transform.up, out hit, _player.SpringDistance))
        {
            isGrounded = true;

            standingRigidbody = hit.rigidbody;
        }
        else
        {
            lastDistance = _player.SpringDistance;
            isGrounded = false;

            standingRigidbody = null;
        }
        return;
        if (isGrounded)
        {
            var springOffset = _player.SpringDistance - hit.distance;
            var springForceToApply = springOffset * _player.SpringForce;

            var springDeltaPerTick = lastDistance - hit.distance;
            var springDelta = springDeltaPerTick / Time.fixedDeltaTime;

            var springDampToApply = springDelta * _player.SpringDamping;

            var forceToApply = springForceToApply + springDampToApply;

            _player.Rigidbody.AddForce(Vector3.up * forceToApply);
            standingRigidbody?.AddForceAtPosition(Vector3.down * forceToApply, hit.point);

            lastDistance = hit.distance;
        }
    }

    private void ApplyAdditiveGravity()
    {
        _player.Rigidbody.AddForce(Vector3.down * _player.AdditiveGravity, ForceMode.Acceleration);
    }


    public override Vector2 GetInputDirectionOverride(Vector2 input)
    {
        var camera = Camera.main;

        var cameraForward = Vector3.ProjectOnPlane(camera.transform.forward, Vector3.up).normalized;
        var cameraRight = Vector3.ProjectOnPlane(camera.transform.right, Vector3.up).normalized;

        var overrideDirection = cameraForward * input.y + cameraRight * input.x;

        return new Vector2(overrideDirection.x, overrideDirection.z);
    }
}
