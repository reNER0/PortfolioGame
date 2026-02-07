using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagePopupCreator : MonoBehaviour
{
    [SerializeField]
    private DamagePopup damagePopupPrefab;

    private void Start()
    {
        GameBus.OnPredictableHit += CreateDamagePopup;
    }

    private void OnDestroy()
    {
        GameBus.OnPredictableHit -= CreateDamagePopup;
    }

    private void CreateDamagePopup(Predictable predictable, int damage) 
    {
        var damagePopup = Instantiate(damagePopupPrefab, predictable.transform.position + Vector3.up, Quaternion.identity);
        damagePopup.SetDamage(damage);
        damagePopup.PlayAnimation();
    }
}
