using UnityEngine;

public class PlayerSound : MonoBehaviour
{
    [SerializeField] private AudioSource footStepAudioSource;
    [SerializeField] private AudioSource gunAudioSource;

    [SerializeField] private AudioClip[] footstepSounds;

    [SerializeField] private AudioClip magazineOutSound;
    [SerializeField] private AudioClip magazineInSound;
    [SerializeField] private AudioClip slideRackSound;

    [SerializeField] private AudioClip weaponEquipSound;

    private int _lastFootstepIndex = -1;


    public void PlayFootstep()
    {
        if (footstepSounds == null || footstepSounds.Length == 0)
            return;

        int index;

        if (footstepSounds.Length == 1)
        {
            index = 0;
        }
        else
        {
            do
            {
                index = Random.Range(0, footstepSounds.Length);
            }
            while (index == _lastFootstepIndex);
        }

        _lastFootstepIndex = index;

        footStepAudioSource.PlayOneShot(footstepSounds[index]);
    }

    public void PlayMagazineOut()
    {
        PlayGunSound(magazineOutSound);
    }

    public void PlayMagazineIn()
    {
        PlayGunSound(magazineInSound);
    }

    public void PlaySlideRack()
    {
        PlayGunSound(slideRackSound);
    }

    public void PlayWeaponEquip()
    {
        PlayGunSound(weaponEquipSound);
    }

    public void PlayGunSound(AudioClip clip)
    {
        if (clip == null || gunAudioSource == null)
            return;

        gunAudioSource.PlayOneShot(clip);
    }
}