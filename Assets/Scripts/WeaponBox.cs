using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBox : MonoBehaviour
{
    [SerializeField]
    private float rotationSpeed = 50f;

    [SerializeField]
    private WeaponModel weaponModel;

    private void OnTriggerEnter(Collider other)
    {
        var player = other.GetComponent<Player>();

        if (player == null)
            return;

        player.WeaponController.PickupWeapon(weaponModel);
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, Time.deltaTime * rotationSpeed);
    }
}
