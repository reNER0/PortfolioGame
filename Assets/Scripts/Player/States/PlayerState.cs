using UnityEngine;

public abstract class PlayerState : State
{
    protected Player _player;

    public PlayerState(Player player)
    {
        _player = player;
    }

    public abstract void OnInput(PlayerInputs playerInputs);

    public virtual void OnAnimatorIK(int layer) { }
    public virtual void OnCollisionEnter(Collision collision) { }
}

