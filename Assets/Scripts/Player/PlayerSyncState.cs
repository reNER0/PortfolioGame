using System;
using UnityEngine;

[Serializable]
public class PlayerSyncState : RigidbodyState
{
    public int Health;
    public float Yaw;
    public float Pitch;

    public PlayerSyncState(int tick, Vector3 position, Vector3 velocity, Quaternion rotation, Vector3 rotationVelocity, int health, float yaw, float pitch) : base(tick, position, velocity, rotation, rotationVelocity)
    {
        Health = health;
        Yaw = yaw;
        Pitch = pitch;
    }
}