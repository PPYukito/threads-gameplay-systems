using UnityEngine;

public class Bullet : MonoBehaviour
{

    [SerializeField] private float bulletSpeed = 10.0f;
    [SerializeField] private GameObject hitParticle;
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private float hitSoundVolume = 3.0f;
    [SerializeField] private Rigidbody rb;

    public void SetBulletVelocity(Quaternion bulletRotation)
    {

        transform.rotation = bulletRotation;

        rb.linearVelocity = transform.forward * bulletSpeed;

    }

    private void OnCollisionEnter(Collision collision)
    {

        IPowerGenerator poweringDevice = collision.collider.GetComponent<IPowerGenerator>();
        if (poweringDevice != null)
            poweringDevice.JumpStart();

        // Use contact point for spawning hit effect;
        ContactPoint contact = collision.GetContact(0);

        //Quaternion particleRotation = Quaternion.Euler(contact.normal);

        float randomRoll = Random.Range(0f, 360f);
        Quaternion hitRotation = Quaternion.LookRotation(contact.normal) * Quaternion.Euler(0f, 0f, randomRoll);

        // Effects
        Instantiate(hitParticle, contact.point, hitRotation);

        #region Just in case I need to use this later, but I don't want to delete it yet
        //    {
        //        public static void PlayClipAtPointCustom(AudioClip clip, Vector3 position, float volume = 1f, float pitch = 1f, float maxDistance = 20f)
        //{
        //    if (clip == null) return;

        //    // Create a temporary GameObject
        //    GameObject audioObj = new GameObject("TempAudio_" + clip.name);
        //    audioObj.transform.position = position;

        //    // Add and configure the AudioSource
        //    AudioSource source = audioObj.AddComponent<AudioSource>();
        //    source.clip = clip;

        //    // This bypasses the 0.0 - 1.0 restriction of PlayClipAtPoint
        //    source.volume = volume;
        //    source.pitch = pitch;

        //    // Configure 3D spatial settings
        //    source.spatialBlend = 1.0f; // 1.0 = Full 3D Sound
        //    source.rolloffMode = AudioRolloffMode.Logarithmic;
        //    source.minDistance = 1f;
        //    source.maxDistance = maxDistance;

        //    // Play the sound
        //    source.Play();

        //    // Automatically destroy the GameObject after the clip finishes
        //    Object.Destroy(audioObj, clip.length);
        //}
        //}
        #endregion

        SoundManager.instance.PlaySoundOnce(hitSound, hitSoundVolume);

        // Destroy the bullet after hitting the object
        Destroy(gameObject);

    }

}