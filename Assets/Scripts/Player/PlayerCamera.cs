using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : Singleton<PlayerCamera>
{
    [SerializeField]
    private Transform upperMount;
    [SerializeField]
    private Transform camera;

    [SerializeField]
    private float height;
    [SerializeField]
    private float angle;
    [SerializeField]
    private float distance;
    [SerializeField]
    private float cameraTurnSpeed;
    [SerializeField]
    private AnimationCurve cameraTurnSpeedMultiplier;

    private Player player;

    private Vector3 lastPosition;


    private void FixedUpdate()
    {
        upperMount.localPosition = Vector3.up * height;
        upperMount.localEulerAngles = new Vector3(angle, 0, 0);
        camera.localPosition = Vector3.back * distance;
        camera.localRotation = Quaternion.identity;

        var deltaPosition = player.Rigidbody.position - lastPosition;
        var speedVector = deltaPosition / Time.deltaTime;
        var direction = speedVector / player.MaxSpeed;

        var camToMountVector = upperMount.position - camera.position;
        camToMountVector.y = 0;

        var camToPlayerVector = player.Rigidbody.position - camera.position;
        camToPlayerVector.y = 0;

        var dotVector = Vector3.Dot(direction, transform.forward);

        var cameraTurnAngle = Vector3.SignedAngle(camToMountVector, camToPlayerVector, Vector3.up);
        var rotationScaleFactor = cameraTurnSpeed * cameraTurnSpeedMultiplier.Evaluate(dotVector);

        transform.Rotate(Vector3.up, cameraTurnAngle * rotationScaleFactor);
        transform.position = player.Rigidbody.position;

        lastPosition = player.Rigidbody.position;
    }


    public void SetTarget(Player newTarget)
    {
        player = newTarget;
    }
}
