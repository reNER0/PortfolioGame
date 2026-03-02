using System;
using System.Linq;
using Assets.Scripts.Network.Commands;
using UnityEditor.Rendering;
using UnityEngine;

public class PlayerStandingState : PlayerState
{
    private DateTime creationTime = DateTime.Now;
    private float sleepTime;

    protected float lastDistance;
    protected bool isGrounded = true;
    protected bool firstGroundTouch = false;
    protected Vector3 currentVelocity;

    protected bool isJumped = false;

    protected Rigidbody standingRigidbody;
    protected RaycastHit hit;

    private Vector3 lastPosition;
    private Vector3 simulatedVelocity;


    public PlayerStandingState(Player player, float sleepTime = 0) : base(player)
    {
        this.sleepTime = sleepTime;
    }


    public override void OnEnter()
    {
        lastDistance = _player.SpringDistance;
    }

    public override void OnUpdate()
    {
        var velocity = (_player.transform.position - lastPosition) / Time.deltaTime;

        if (standingRigidbody != null)
            velocity -= standingRigidbody.velocity;

        Vector3 localVelocity = _player.transform.InverseTransformDirection(velocity);

        simulatedVelocity = Vector3.Lerp(simulatedVelocity, localVelocity, 1/15f);

        _player.Animator.SetFloat("VelocityX", simulatedVelocity.x / _player.MaxSpeed);
        _player.Animator.SetFloat("VelocityY", simulatedVelocity.z / _player.MaxSpeed);
        _player.Animator.SetBool("IsGrounded", isGrounded);

        lastPosition = _player.transform.position;
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

    /*
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
    */
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
        ApplyMoveForce(playerInputs.X, playerInputs.Y);

        if (!isGrounded)
            ApplyAdditiveGravity();

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

    protected virtual void ApplyMoveForce(float x, float y)
    {
        ApplyTargetVelocity(Vector3.zero);
    }

    protected void ApplyTargetVelocity(Vector3 targetVelocity)
    {
        if (standingRigidbody != null)
            targetVelocity += standingRigidbody.GetPointVelocity(hit.point);

        var velocity = _player.Rigidbody.velocity;

        velocity.y = 0;
        targetVelocity.y = 0;

        var dotVector = Vector3.Dot(targetVelocity.normalized, velocity.normalized);

        var acceleration = (isGrounded ? _player.MaxAcceleration : _player.AirAcceleration) * _player.ReverseAccelerationMultiplierCurve.Evaluate(dotVector);

        currentVelocity = Vector3.MoveTowards(velocity, targetVelocity, acceleration * Time.fixedDeltaTime);

        var accelerationToApply = (currentVelocity - velocity) / Time.fixedDeltaTime;

        _player.Rigidbody.AddForce(accelerationToApply * _player.Rigidbody.mass);
    }

    private void ApplySpringForce()
    {
        if (Physics.Raycast(_player.transform.position + _player.transform.up, -_player.transform.up, out hit, _player.SpringDistance, _player.WalkableLayerMask, QueryTriggerInteraction.Ignore))
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
