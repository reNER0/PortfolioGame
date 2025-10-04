using System.Linq;
using Assets.Scripts.Network.Commands;
using UnityEditor.Rendering;
using UnityEngine;

public class PlayerWalkingState : PlayerState
{
    private float lastDistance;
    private bool isGrounded;
    private Vector3 currentVelocity;


    private bool isJumped = true;


    public PlayerWalkingState(Player player) : base(player) { }


    public override void OnEnter()
    {

    }

    public override void OnUpdate()
    {
        Vector3 localVelocity = _player.transform.InverseTransformDirection(_player.Rigidbody.velocity);

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
        if (!NetworkRepository.IsCurrentClientOwnerOfObject(_player))
            return;

        var car = collision.gameObject.GetComponent<Car>();

        if (car == null)
            return;

        var seat = car.GetNearestSeat(_player.transform.position);

        if (seat == null)
            return;

        var carId = NetworkRepository.NetworkObjectById.First(x => x.Predictable == car).Id;
        var seatId = car.GetSeatId(seat);

        var jumpInCarCmd = new JumpInCarCmd(NetworkRepository.CurrentObjectId, carId, seatId);

        NetworkBus.OnPerformCommand?.Invoke(jumpInCarCmd);
    }

    public override void OnInput(PlayerInputs playerInputs)
    {
        if (_player.Rigidbody.velocity.y <= 0)
            isJumped = false;

        if (!isJumped)
            ApplySpringForce();

        ApplyMoveForce(playerInputs.X, playerInputs.Y);

        Rotate(playerInputs.X, playerInputs.Y);

        if(isGrounded && playerInputs.Jump)
            Jump();

        if (!isGrounded)
            ApplyAdditiveGravity();
    }

    public override void OnExit()
    {

    }


    private void ApplySpringForce()
    {
        RaycastHit hit;

        if (Physics.Raycast(_player.transform.position + _player.transform.up, -_player.transform.up, out hit, _player.SpringDistance))
        {
            isGrounded = true;
        }
        else
        {
            lastDistance = _player.SpringDistance;
            isGrounded = false;
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

            lastDistance = hit.distance;

        }
    }

    private void ApplyMoveForce(float x, float y)
    {
        /*
        var camera = Camera.main;

        var cameraForward = Vector3.ProjectOnPlane(camera.transform.forward, Vector3.up).normalized;
        var cameraRight = Vector3.ProjectOnPlane(camera.transform.right, Vector3.up).normalized;


        var moveDirection = cameraForward * y + cameraRight * x;
        */
        var moveDirection = Vector3.forward * y + Vector3.right * x;

        moveDirection = Vector3.ClampMagnitude(moveDirection, 1);

        var targetVelocity = moveDirection * _player.MaxSpeed;

        var velocity = _player.Rigidbody.velocity;
        velocity.y = 0;

        var dotVector = Vector3.Dot(moveDirection, velocity.normalized);

        var acceleration = (isGrounded ? _player.MaxAcceleration : _player.AirAcceleration) * _player.ReverseAccelerationMultiplierCurve.Evaluate(dotVector);

        currentVelocity = Vector3.MoveTowards(velocity, targetVelocity, acceleration * Time.fixedDeltaTime);

        var accelerationToApply = (currentVelocity - velocity) / Time.fixedDeltaTime;

        _player.Rigidbody.AddForce(accelerationToApply * _player.Rigidbody.mass);
    }

    private void Rotate(float x, float y)
    {
        if (x == 0 && y == 0)
            return;

        Vector3 targetDir = _player.Rigidbody.velocity;

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
