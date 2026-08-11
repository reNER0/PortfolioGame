using UnityEngine;

public enum FireMode
{
    Automatic,
    SemiAuto,
    Manual
}

public enum ReloadMode
{
    Magazine,
    Bullet
}


public abstract class WeaponModel : ScriptableObject
{
    public WeaponPrefab prefab;

    public int damage = 10;
    public float range = 100;
    public float fireRate;
    public bool isTwoHanded;

    public AudioClip attackSound;
}