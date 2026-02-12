using System;
using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    public Action OnReloadAnimationFinished;
    public Action OnPrepareAnimationFinished;
    public Action OnAnimationCombo;

    public void ReloadAnimationFinished()
    {
        OnReloadAnimationFinished?.Invoke();
    }

    public void PrepareAnimationFinished()
    {
        OnPrepareAnimationFinished?.Invoke();
    }

    public void Combo()
    {
        OnAnimationCombo?.Invoke();
    }
}
