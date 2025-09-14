using System;
using System.Linq;
using UnityEngine;


[Serializable]
public struct IKTraget
{
    public AvatarIKGoal avatarIKGoal;
    public Transform target;
}


public class Seat : MonoBehaviour
{
    [SerializeField]
    private SphereCollider sphereCollider;
    [SerializeField]
    private Transform exitTransform;
    [SerializeField]
    private Vector3 exitVector;
    [SerializeField]
    private IKTraget[] ikTargets;

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
    public Vector3 GetExitVector()
    {
        return exitVector;
    }

    public Transform GetIKTransform(AvatarIKGoal avatarIKGoal)
    {
        return ikTargets.First(x => x.avatarIKGoal == avatarIKGoal).target;
    }
}