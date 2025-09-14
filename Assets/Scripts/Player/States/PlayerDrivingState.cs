using System.Linq;
using UnityEngine;
using DG.Tweening;


public class PlayerDrivingState : PlayerState
{
    private Car _car;
    private Seat _seat;

    private bool _isSit;


    public PlayerDrivingState(Player player, Car car, Seat seat) : base(player)
    {
        _car = car;
        _seat = seat;
    }


    public override void OnEnter()
    {
        _player.Rigidbody.isKinematic = true;
        _player.GetComponent<Collider>().enabled = false;

        _player.transform.parent = _seat.transform;

        _player.Animator.SetTrigger("JumpInCar");

        float jumpForce = Mathf.Max(2 - (_player.transform.position.y - _seat.transform.position.y), 0);

        _player.transform.DOLocalJump(Vector3.zero, jumpForce, 1, 1/2f).SetEase(Ease.Linear).OnComplete(OnSit);
        _player.transform.DOLocalRotate(Vector3.zero, 1/2f).SetEase(Ease.Linear);


        void OnSit()
        {
            _isSit = true;
            _player.Animator.SetTrigger("SitInCar");
        }
    }

    public override void OnUpdate()
    {
        Vector3 localVelocity = _player.transform.InverseTransformDirection(_player.Rigidbody.velocity);

        _player.Animator.SetFloat("VelocityX", localVelocity.x);
        _player.Animator.SetFloat("VelocityY", localVelocity.z);
    }

    public override void OnAnimatorIK(int layer)
    {
        if (!_isSit)
            return;

        if (!_player.UseIK)
            return;

        SetIK(AvatarIKGoal.LeftFoot);
        SetIK(AvatarIKGoal.RightFoot);
        SetIK(AvatarIKGoal.LeftHand);
        SetIK(AvatarIKGoal.RightHand);
    }

    private void SetIK(AvatarIKGoal avatarIKGoal)
    {
        _player.Animator.SetIKPositionWeight(avatarIKGoal, 1);
        _player.Animator.SetIKRotationWeight(avatarIKGoal, 1);

        _player.Animator.SetIKPosition(avatarIKGoal, _seat.GetIKTransform(avatarIKGoal).position);
    }

    public override void OnInput(PlayerInputs playerInputs)
    {
        _car.Input(playerInputs);

        if (!playerInputs.Jump)
            return;

        var leaveCar = new LeaveCarCmd(NetworkRepository.CurrentObjectId, NetworkRepository.NetworkObjectById.First(x => x.Predictable == _car).Id, _car.GetSeatId(_seat));

        NetworkBus.OnPerformCommand?.Invoke(leaveCar);
        //NetworkBus.OnCommandSendToServer?.Invoke(leaveCar);
    }

    public override void OnExit()
    {
        _player.transform.DOKill();

        _player.Animator.SetTrigger("JumpFromCar");
         
        _player.GetComponent<IgnoreCollider>().SetIgnoreCollider(_car.GetComponent<Collider>());

        _player.transform.parent = null;
        _player.Rigidbody.isKinematic = false;
        _player.Rigidbody.velocity = _car.Rigidbody.velocity;
        Vector3 localExitVector = _player.transform.TransformDirection(_seat.GetExitVector());
        _player.Rigidbody.AddForce(localExitVector * _player.Rigidbody.mass , ForceMode.Impulse);


        _player.GetComponent<Collider>().enabled = true;
    }
}
