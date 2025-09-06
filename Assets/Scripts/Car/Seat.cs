using UnityEngine;

public class Seat : MonoBehaviour
{
    [SerializeField]
    private SphereCollider sphereCollider;
    [SerializeField]
    private Transform exitTransform;

    public Player Player { get; private set; }

    public float SeatableRadius => sphereCollider.radius;


    public void SetPlayer(Player player)
    {
        Player = player;
    }

    public Vector3 GetExitPoint()
    {
        return exitTransform.position;
    }
}