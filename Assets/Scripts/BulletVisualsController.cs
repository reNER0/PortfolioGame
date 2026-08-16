using DG.Tweening;
using UnityEngine;


// TODO : make an object pool
public class BulletVisualsController : MonoBehaviour
{
    public TrailRenderer TrailRendererPrefab;
    public ParticleSystem HitEffectPrefab;

    private void Start()
    {
        GameBus.OnBulletFX += OnBulletFX;
    }

    private void OnBulletFX(BulletFX bulletFX)
    {
        var trail = Instantiate(TrailRendererPrefab, bulletFX.StartPosition, Quaternion.identity);
        trail.transform.DOMove(bulletFX.EndPosition, 0.05f).OnComplete(OnHit);

        void OnHit()
        {
            Destroy(trail.gameObject, trail.time);

            var hitNormal = bulletFX.HitNormal;
            var hitRotation = hitNormal.sqrMagnitude > 0.0001f
                ? Quaternion.LookRotation(hitNormal)
                : Quaternion.identity;

            Instantiate(HitEffectPrefab, bulletFX.EndPosition, hitRotation);
        }
    }

    private void OnDestroy()
    {
        GameBus.OnBulletFX -= OnBulletFX;
    }
}

public class BulletFX
{
    public Vector3 StartPosition;
    public Vector3 EndPosition;
    public Vector3 HitNormal;
}
