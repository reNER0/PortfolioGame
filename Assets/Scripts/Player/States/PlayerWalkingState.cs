using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerWalkingState : PlayerStandingState
{
    public PlayerWalkingState(Player player, float sleepTime) : base(player, sleepTime) { }


    public override void OnEnter()
    {
        base.OnEnter();
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
        Rotate(playerInputs.X, playerInputs.Y);

        if (isGrounded && playerInputs.Jump)
            Jump();

        base.OnInput(playerInputs);
    }

    public override void OnExit()
    {

    }

    protected override void ApplyMoveForce(float x, float y)
    {
        var moveDirection = Vector3.forward * y + Vector3.right * x;

        moveDirection = Vector3.ClampMagnitude(moveDirection, 1);

        var targetVelocity = moveDirection * _player.MaxSpeed;

        ApplyTargetVelocity(targetVelocity);
    }

    private void Rotate(float x, float y)
    {
        //bool isAiming = _player.WeaponController.IsAiming();

        if (x == 0 && y == 0)
            return;

        Vector3 targetDir = _player.Rigidbody.velocity;

        //if (isAiming)
        //    targetDir = PlayerCamera.Instance.transform.forward;

        if (standingRigidbody != null)
            targetDir -= standingRigidbody.velocity;

        targetDir.y = 0f;

        if (targetDir.magnitude < 0.01f)
            return;

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
