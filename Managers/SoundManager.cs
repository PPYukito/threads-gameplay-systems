using UnityEngine;

public class SoundManager : MonoBehaviour
{

    public static SoundManager instance { get; private set; }

    [SerializeField] private AudioSource effectAudioSource;

    private void Awake()
    {

        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

    }

    public void PlaySoundOnce(AudioClip clip, float volume)
    {
        if (effectAudioSource == null)
            return;

        effectAudioSource.PlayOneShot(clip, volume);
    }

}
