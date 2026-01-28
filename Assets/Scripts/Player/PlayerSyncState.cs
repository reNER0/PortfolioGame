using System;
using UnityEngine;

[Serializable]
public class PlayerSyncState : RigidbodyState
{
    public int _health;

    public PlayerSyncState(int tick, Vector3 position, Vector3 velocity, Quaternion rotation, Vector3 rotationVelocity, PlayerInputs playerInputs, int health) : base(tick, position, velocity, rotation, rotationVelocity, playerInputs)
    {
        _health = health;
    }
}