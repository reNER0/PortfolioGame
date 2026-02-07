using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class BadEffectPanel : MonoBehaviour
{
    [SerializeField]
    private Image image;
    [SerializeField]
    private float fadeDuration = 0.5f;
    [SerializeField]
    private float fade = 0.5f;


    private void Start()
    {
        GameBus.OnBadEffect += OnBadEffect;
    }

    private void OnDestroy()
    {
        GameBus.OnBadEffect -= OnBadEffect;
    }

    private void OnBadEffect()
    {
        image.DOKill();
        image.DOFade(fade, 0);
        image.DOFade(0, fadeDuration);
    }
}
