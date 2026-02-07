using System;
using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    public Action OnReloadAnimationFinished;
    public Action OnPrepareAnimationFinished;

    public void ReloadAnimationFinished()
    {
        OnReloadAnimationFinished?.Invoke();
    }

    public void PrepareAnimationFinished()
    {
        OnPrepareAnimationFinished?.Invoke();
    }
}
