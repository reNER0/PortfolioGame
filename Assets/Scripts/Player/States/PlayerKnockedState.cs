using UnityEngine;

public class PlayerKnockedState : PlayerStandingState
{
    private float knockedTime = 1/3f;
    private float knockBackForce = 3/10f;
    private Vector3 knockVector;

    public PlayerKnockedState(Player player, Vector3 knockVector) : base(player)
    {
        this.knockVector = knockVector;
    }


    public override void OnEnter()
    {
        base.OnEnter();
    }

    public override void OnInput(PlayerInputs playerInputs)
    {
        base.OnInput(playerInputs);

        _player.transform.forward = -knockVector;

        knockedTime -= Time.fixedDeltaTime;

        if (knockedTime > 0)
            return;

        _player.PlayerStateMachine.ChangeState(new PlayerWalkingState(_player, 0));
    }



    protected override void ApplyMoveForce(float x, float y)
    {
        var moveDirection = knockVector;

        moveDirection = Vector3.ClampMagnitude(moveDirection, knockBackForce);

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
    }



    public override void OnExit()
    {
        base.OnExit();
    }
}
