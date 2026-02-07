using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Tools
{
    public static void YawPitchFromDirection(Vector3 direction, out float yaw, out float pitch)
    {
        direction.Normalize();

        // yaw: 0..360
        yaw = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg; // -180..180
        if (yaw < 0f) yaw += 360f;

        // pitch: -90..90
        pitch = Mathf.Asin(Mathf.Clamp(direction.y, -1f, 1f)) * Mathf.Rad2Deg;
        pitch = Mathf.Clamp(pitch, -89f, 89f);
    }

    public static Vector3 DirectionFromYawPitch(float yaw, float pitch)
    {
        float yawRad = yaw * Mathf.Deg2Rad;
        float pitchRad = pitch * Mathf.Deg2Rad;

        float cosPitch = Mathf.Cos(pitchRad);

        Vector3 dir = new Vector3(
            Mathf.Sin(yawRad) * cosPitch, // X
            Mathf.Sin(pitchRad),          // Y
            Mathf.Cos(yawRad) * cosPitch  // Z
        );

        return dir.normalized;
    }
}
