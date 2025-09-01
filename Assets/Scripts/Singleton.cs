using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Singleton<T> : MonoBehaviour
{
    private static T instance;

    private void Awake()
    {
        instance = GetComponent<T>();
    }

    public static T Instance => instance;
}