using Assets.Scripts.Commands;
using System;
using UnityEngine;

public class MeleeTrigger : MonoBehaviour
{
    public Action<IDamagable> OnMeleeHit;

    private void OnTriggerEnter(Collider other)
    {
        var damagable = other.GetComponent<IDamagable>();

        if (damagable == null)
            return;

        OnMeleeHit?.Invoke(damagable);
    }
}