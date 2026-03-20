using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FenceGenerator : MonoBehaviour
{
    [SerializeField]
    private GameObject prefab;
    [SerializeField]
    private float delta;
    [SerializeField]
    private int count;
    [SerializeField]
    private Vector3 direction;

    private void Awake()
    {
        for (int i = 1; i <= count; i++) 
        {
            var newPos = transform.position + direction * delta * i;

            Instantiate(prefab, newPos, transform.rotation, transform.parent);
        }
    }
}
