using System;
using System.Linq;
using Assets.Scripts.Network.Commands;
using UnityEditor.Rendering;
using UnityEngine;

public class PlayerWalkingState : PlayerStandingState
{
    private DateTime creationTime = DateTime.Now;
    private float sleepTime = 1 / 2f;

    private bool firstGroundTouch = false;
    private Vector3 currentVelocity;

    private bool isAiming;

    private Rigidbody standingRigidbody;
    private RaycastHit hit;


    public PlayerWalkingState(Player player) : base(player) { }


    public override void OnEnter()
    {
        base.OnEnter();
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

    public override void OnCollisionEnter(Collision collision)
    {
        if (!NetworkRepository.Current.IsCurrentClientOwnerOfObject(_player))
            return;

        var car = collision.gameObject.GetComponent<Car>();

        if (car == null)
            return;

        var seat = car.GetNearestSeat(_player.transform.position);

        if (seat == null)
            return;

        var carId = NetworkRepository.Current.NetworkObjectById.First(x => x.Predictable == car).Id;
        var seatId = car.GetSeatId(seat);

        var jumpInCarCmd = new JumpInCarCmd(NetworkRepository.Current.CurrentObjectId, carId, seatId);

        NetworkBus.OnPerformCommand?.Invoke(jumpInCarCmd);
    }

    public override void OnInput(PlayerInputs playerInputs)
    {
        ApplyMoveForce(playerInputs.X, playerInputs.Y);

        Rotate(playerInputs.X, playerInputs.Y);

        if (isGrounded && playerInputs.Jump)
            Jump();

        base.OnInput(playerInputs);
    }

    public override void OnExit()
    {

    }

    private void ApplyMoveForce(float x, float y)
    {
        var moveDirection = Vector3.forward * y + Vector3.right * x;

        moveDirection = Vector3.ClampMagnitude(moveDirection, 1);

        var targetVelocity = moveDirection * _player.MaxSpeed;

        if (standingRigidbody != null)
            targetVelocity += standingRigidbody.GetPointVelocity(hit.point);

        var velocity = _player.Rigidbody.velocity;

        velocity.y = 0;
        targetVelocity.y = 0;

        var dotVector = Vector3.Dot(moveDirection, velocity.normalized);

        var acceleration = (isGrounded ? _player.MaxAcceleration : _player.AirAcceleration) * _player.ReverseAccelerationMultiplierCurve.Evaluate(dotVector);

        currentVelocity = Vector3.MoveTowards(velocity, targetVelocity, acceleration * Time.fixedDeltaTime);

        var accelerationToApply = (currentVelocity - velocity) / Time.fixedDeltaTime;

        _player.Rigidbody.AddForce(accelerationToApply * _player.Rigidbody.mass);

        // TODO : add this if want player to move standing rigidbody
        // disabled this bacause was very laggy
        //standingRigidbody?.AddForceAtPosition(-accelerationToApply * _player.Rigidbody.mass, hit.point);
    }

    private void Rotate(float x, float y)
    {
        bool isAiming = _player.WeaponController.IsAiming();

        if (x == 0 && y == 0 && !isAiming)
            return;

        Vector3 targetDir = _player.Rigidbody.velocity;

        if (isAiming)
            targetDir = PlayerCamera.Instance.transform.forward;

        if (standingRigidbody != null)
            targetDir -= standingRigidbody.velocity;

        targetDir.y = 0f;

        Quaternion targetRot = Quaternion.LookRotation(targetDir);
        _player.transform.rotation = Quaternion.Lerp(_player.transform.rotation, targetRot, _player.MaxAcceleration * Time.fixedDeltaTime);
    }

    private void Jump()
    {
        isJumped = true;
        isGrounded = false;
        lastDistance = _player.SpringDistance;

        _player.Rigidbody.AddForce(Vector3.up * _player.JumpForce * _player.Rigidbody.mass, ForceMode.Impulse);
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
