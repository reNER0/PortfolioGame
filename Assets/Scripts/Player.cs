using UnityEngine;

public class Player : PhysicsObject
{
    [SerializeField]
    private float jumpForce;
    [SerializeField]
    private float maxSpeed;
    [SerializeField]
    private float maxAcceleration;
    [SerializeField]
    private AnimationCurve reverseAccelerationMultiplierCurve;

    [SerializeField]
    private float springDistance;
    [SerializeField]
    private float springForce;
    [SerializeField]
    private float springDamping;

    private float lastDistance;
    private bool isGrounded;
    private Vector3 currentVelocity;


    // same as FixedUpdate
    public override void Input(PlayerInputs playerInputs)
    {
        ApplySpringForce();
        ApplyMoveForce(playerInputs.X, playerInputs.Y);
        Rotate(playerInputs.X, playerInputs.Y);
        ApplyJumpForce(playerInputs.Jump);
    }

    private void ApplySpringForce()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, -transform.up, out hit, springDistance))
        {
            isGrounded = true;
        }
        else
        {
            lastDistance = springDistance;
            isGrounded = false;
        }
        

        if (isGrounded)
        {
            var springOffset = springDistance - hit.distance;
            var springForceToApply = springOffset * springForce;

            var springDeltaPerTick = lastDistance - hit.distance;
            var springDelta = springDeltaPerTick / Time.fixedDeltaTime;

            var springDampToApply = springDelta * springDamping;

            var forceToApply = springForceToApply + springDampToApply;

            Rigidbody.AddForce(Vector3.up * forceToApply);

            lastDistance = hit.distance;

        }
    }

    private void ApplyMoveForce(float x, float y)
    {
        var camera = Camera.main;

        var cameraForward = Vector3.ProjectOnPlane(camera.transform.forward, Vector3.up).normalized;
        var cameraRight = Vector3.ProjectOnPlane(camera.transform.right, Vector3.up).normalized;


        var moveDirection = cameraForward * y + cameraRight * x;

        var targetVelocity = moveDirection * maxSpeed;

        var velocity = Rigidbody.velocity;
        velocity.y = 0;

        var dotVector = Vector3.Dot(moveDirection, velocity.normalized);

        var acceleration = maxAcceleration * reverseAccelerationMultiplierCurve.Evaluate(dotVector);

        currentVelocity = Vector3.MoveTowards(velocity, targetVelocity, acceleration * Time.fixedDeltaTime);

        var accelerationToApply = (currentVelocity - velocity) / Time.fixedDeltaTime;

        Rigidbody.AddForce(accelerationToApply * Rigidbody.mass);
    }

    private void Rotate(float x, float y)
    {
        if (x == 0 && y == 0)
            return;

        Vector3 targetDir = Rigidbody.velocity;

        targetDir.y = 0f;

        Quaternion targetRot = Quaternion.LookRotation(targetDir);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, maxAcceleration * Time.fixedDeltaTime);
    }

    private void ApplyJumpForce(bool jump)
    {
        if (!jump)
            return;

        Rigidbody.AddForce(Vector3.up * jumpForce * Rigidbody.mass, ForceMode.Impulse);
    }
}
