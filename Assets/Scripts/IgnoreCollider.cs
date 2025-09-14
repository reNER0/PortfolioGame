using System.Runtime.InteropServices;
using UnityEngine;

public class IgnoreCollider : MonoBehaviour
{
    private float safeBuffer = 0.3f;

    private Collider Collider1;
    private Collider Collider2;

    private void Awake()
    {
        Collider1 = GetComponent<Collider>();
    }

    private void FixedUpdate()
    {
        if (Collider2 == null)
            return;

        if (!Separated(Collider1, Collider2))
            return;

        Physics.IgnoreCollision(Collider1, Collider2, false);
        Collider2 = null;
    }

    public void SetIgnoreCollider(Collider collider)
    {
        Physics.IgnoreCollision(Collider1, collider, true);

        Collider2 = collider;
    }


    bool Separated(Collider a, Collider b)
    {
        if (!a || !b) return true;

        Vector3 dir; float dist;
        if (Physics.ComputePenetration(
                a, a.transform.position, a.transform.rotation,
                b, b.transform.position, b.transform.rotation,
                out dir, out dist))
            return false;

        var pa = a.ClosestPoint(b.bounds.center);
        var pb = b.ClosestPoint(a.bounds.center);
        return (pa - pb).sqrMagnitude >= safeBuffer * safeBuffer;
    }
}
