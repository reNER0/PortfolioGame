using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShotProcessor : MonoBehaviour
{
    [Header("Links")]
    [SerializeField] private Camera playerCamera;     // Камера, по центру которой прицел
    [SerializeField] private Transform muzzle;        // Дуло (точка, откуда визуально летит выстрел)

    [SerializeField] private static float maxDistance = 200f;

    /// <summary>
    /// Делает хитскан-выстрел: целимся камерой в центр экрана, стреляем лучом из дула в точку прицеливания.
    /// Возвращает true если что-то попали.
    /// </summary>
    public bool Shoot(out RaycastHit hit, out Vector3 origin, out Vector3 direction)
    {
        hit = default;

        // 1) Находим точку прицеливания лучом из камеры (центр экрана)
        var camOrigin = playerCamera.transform.position;
        var camDir = playerCamera.transform.forward;

        Vector3 aimPoint;
        if (Physics.Raycast(camOrigin, camDir, out var aimHit, maxDistance))
            aimPoint = aimHit.point;
        else
            aimPoint = camOrigin + camDir * maxDistance;

        // 2) Формируем "настоящий" луч выстрела из дула в aimPoint
        origin = muzzle ? muzzle.position : camOrigin;

        direction = (aimPoint - origin);
        if (direction.sqrMagnitude < 0.000001f)
            direction = camDir;
        else
            direction.Normalize();

        // 3) Финальный raycast выстрела (именно его обычно и надо на сервер отправлять: origin + direction)
        bool didHit = Physics.Raycast(origin, direction, out hit, maxDistance);

        return didHit;
    }

    public static Collider GetHit(Vector3 origin, Vector3 direction) 
    {
        Physics.Raycast(origin, direction, out var hit, maxDistance);

        return hit.collider;
    }

    // Пример вызова: ЛКМ
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Shoot(out var hit, out var origin, out var dir))
            {
                // Локальная реакция (например, декали/эффекты)
                Debug.Log($"Hit: {hit.collider.name} at {hit.point}");

                // В мультиплеере сюда: отправка на сервер
                var shotCmd = new ShotCmd(NetworkTime.CurrentTick, origin, dir);
                NetworkBus.OnPerformCommand?.Invoke(shotCmd);
            }
            else
            {
                // В мультиплеере тоже отправляй "промах", чтобы у других был трассер/эффект
                // SendShoot(origin, dir, clientTick);
            }
        }
    }
}
