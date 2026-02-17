using System.Linq;
using UnityEngine;

public class WeaponBox : MonoBehaviour
{
    [SerializeField]
    private float rotationSpeed = 50f;

    [SerializeField]
    private WeaponModel weaponModel;

    private void OnTriggerEnter(Collider other)
    {
        if (!NetworkRepository.Current.IsServer)
            return;

        var player = other.GetComponent<Player>();

        if (player == null)
            return;

        var equipCmd = new EquipWeaponCmd(NetworkRepository.Current.NetworkObjectById.First(x => x.Predictable == player).Id, weaponModel.name);

        NetworkBus.OnPerformCommand?.Invoke(equipCmd);
    }

    private void Update()
    {
        transform.Rotate(Vector3.up, Time.deltaTime * rotationSpeed);
    }
}