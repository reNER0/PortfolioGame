using Assets.Scripts.Commands;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField]
    private RectTransform healthBar;

    private IHealth health;

    private void Start()
    {
        health = GetComponentInParent<IHealth>();

        SetHealthPercent(health.GetHealth());

        health.HealthChanged += SetHealthPercent;
    }

    private void OnDestroy()
    {
        health.HealthChanged -= SetHealthPercent;
    }

    private void SetHealthPercent(int percent)
    {
        healthBar.localScale = new Vector2(percent / 100f, 1);
    }
}
