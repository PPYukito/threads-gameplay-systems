using UnityEngine;

public class Door : ReceiverDevice
{

    [Header("Door Audio Details")]
    [SerializeField] private AudioSource doorAudioSource;
    [SerializeField] private AudioClip doorOpenSound;
    [SerializeField] private AudioClip doorOpeningSound;
    [SerializeField] private AudioClip doorFinishedOpenSound;

    public void PlayDoorOpenSound() => SoundManager.instance.PlaySoundOnce(doorOpenSound, 3.0f);
    public void PlayDoorOpeningSound() => PlayDoorSound(doorOpeningSound);
    public void PlayDoorFinishedOpenSound()
    {
        doorAudioSource.Stop();
        SoundManager.instance.PlaySoundOnce(doorFinishedOpenSound, 1.0f);
    }

    private void PlayDoorSound(AudioClip clip)
    {
        doorAudioSource.Stop();
        doorAudioSource.clip = clip;
        doorAudioSource.Play();
    }

}
