using TMPro;
using UnityEngine;
using DG.Tweening;

public class DamagePopup : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI damageText;
    [SerializeField] 
    private float lifeTime = 0.8f;
    [SerializeField] 
    private float floatUp = 60f;
    [SerializeField] 
    private float punchScale = 0.25f;
    [SerializeField]
    private float punchTime = 0.15f;

    private Sequence _seq;


    public void PlayAnimation()
    {
        _seq?.Kill();

        _seq = DOTween.Sequence();


        _seq.Append(transform.DOPunchScale(
            Vector3.one / 100f * punchScale,
            punchTime
        ));

        _seq.Join(transform.DOMove(
            transform.position + Vector3.up * floatUp,
            lifeTime
        ).SetEase(Ease.OutCubic));

        _seq.Join(damageText.DOFade(
            0,
            lifeTime
        ).SetEase(Ease.InCubic));

        //_seq.OnComplete(ReturnToPool);
        _seq.OnComplete(() => Destroy(gameObject));
    }

    public void SetDamage(int damage)
    {
        damageText.text = damage.ToString();
    }
}
