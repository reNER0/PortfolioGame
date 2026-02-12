using DG.Tweening;
using UnityEngine;


// TODO : make an object pool
public class BulletVisualsController : MonoBehaviour
{
    public TrailRenderer TrailRendererPrefab;

    private void Start()
    {
        GameBus.OnBulletFX += OnBulletFX;
    }

    private void OnBulletFX(BulletFX bulletFX) 
    {
        var trail = Instantiate(TrailRendererPrefab, bulletFX.StartPosition, Quaternion.identity);
        trail.transform.DOMove(bulletFX.EndPosition, 0.05f).OnComplete(() => { Destroy(trail.gameObject, trail.time); });
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
}