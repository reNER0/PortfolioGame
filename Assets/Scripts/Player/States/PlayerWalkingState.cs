using UnityEngine;

public class PlayerWalkingState : PlayerState
{
    private float lastDistance;
    private bool isGrounded;
    private Vector3 currentVelocity;


    public PlayerWalkingState(Player player) : base(player) { }


    public override void OnEnter()
    {

    }

    public override void OnUpdate()
    {
        _player.Animator.SetFloat("Speed", _player.Rigidbody.velocity.magnitude / _player.MaxSpeed);
    }

    public override void OnInput(PlayerInputs playerInputs)
    {
        ApplySpringForce();
        ApplyMoveForce(playerInputs.X, playerInputs.Y);
        Rotate(playerInputs.X, playerInputs.Y);
        ApplyJumpForce(playerInputs.Jump);
    }

    public override void OnExit()
    {

    }


    private void ApplySpringForce()
    {
        RaycastHit hit;

        if (Physics.Raycast(_player.transform.position + _player.transform.up, -_player.transform.up, out hit, _player.SpringDistance + 1))
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
        var camera = Camera.main;

        var cameraForward = Vector3.ProjectOnPlane(camera.transform.forward, Vector3.up).normalized;
        var cameraRight = Vector3.ProjectOnPlane(camera.transform.right, Vector3.up).normalized;


        var moveDirection = cameraForward * y + cameraRight * x;

        moveDirection = Vector3.ClampMagnitude(moveDirection, 1);

        var targetVelocity = moveDirection * _player.MaxSpeed;

        var velocity = _player.Rigidbody.velocity;
        velocity.y = 0;

        var dotVector = Vector3.Dot(moveDirection, velocity.normalized);

        var acceleration = _player.MaxAcceleration * _player.ReverseAccelerationMultiplierCurve.Evaluate(dotVector);

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

    private void ApplyJumpForce(bool jump)
    {
        if (!jump)
            return;

        _player.Rigidbody.AddForce(Vector3.up * _player.JumpForce * _player.Rigidbody.mass, ForceMode.Impulse);
    }
}
