using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField]
    private AudioClip[] musicPool;

    [SerializeField]
    private AudioSource source;

    private void Start()
    {
        source.clip = musicPool[Random.Range(0, musicPool.Length)];
        source.Play();
    }
}
