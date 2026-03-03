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
    private float zoomMultiplier;
    [SerializeField]
    private float sensitivity;
    [SerializeField]
    private float height;
    [SerializeField]
    private float side;
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
    private bool fixedState;
    private float currentAngle;
    private bool isZooming;

    private void FixedUpdate()
    {
        if (player == null)
            return;

        upperMount.localPosition = Vector3.up * height;

        if (!fixedState)
        {
            upperMount.localPosition += Vector3.right * side;

            Vector2 look = PlayerInputController.inputSystem.Inputs.Look.ReadValue<Vector2>();

            float mouseX = look.x * sensitivity;
            float mouseY = look.y * sensitivity;

            // Вертикаль (камера)
            currentAngle -= mouseY;
            currentAngle = Mathf.Clamp(currentAngle, -90, 90);

            // Горизонталь (игрок)
            transform.Rotate(Vector3.up * mouseX);
        }

        upperMount.localEulerAngles = new Vector3(currentAngle, 0, 0);
        camera.localPosition = Vector3.back * (isZooming ? distance * zoomMultiplier : distance);
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

        if (fixedState)
            transform.Rotate(Vector3.up, cameraTurnAngle * rotationScaleFactor);

        transform.position = player.Rigidbody.position;

        lastPosition = player.Rigidbody.position;
    }

    public void Zoom(bool zoom)
    {
        isZooming = zoom;
    }

    public Vector3 GetLookPoint()
    {
        float maxDistance = 100;

        // 1) Направление прицеливания (куда смотрит камера)
        Vector3 viewDir = camera.transform.forward;

        // 3) Луч прицеливания
        Ray viewRay = new Ray(camera.transform.position, viewDir);


        // 4) Ищем точку прицеливания
        if (Physics.Raycast(viewRay, out RaycastHit hit, maxDistance))
        {
            Debug.DrawLine(viewRay.origin, hit.point, Color.blue);
            return hit.point;
        }

        // 5) Если ни во что не попали — точка на максимальной дистанции
        return camera.transform.position + viewDir * maxDistance;
    }

    public void SetTarget(Player newTarget)
    {
        player = newTarget;
    }

    public void SetState(bool isFixed)
    {
        fixedState = isFixed;

        if (fixedState)
            return;

        currentAngle = angle;
    }
}
