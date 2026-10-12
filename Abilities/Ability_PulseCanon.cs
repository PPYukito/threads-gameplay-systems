using Unity.Cinemachine;
using UnityEngine;

public class Ability_PulseCanon : AbilityEntity
{

    [Header("Shoot Details")]
    [SerializeField] private float aimDistance = 100.0f;
    [SerializeField] private LayerMask hitLayer;

    // To do
    // Add delay after each shoot, so player can't spam shoot pulse canon.


    public override void ActivateAbility()
    {

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        Quaternion aimAtTargetRotation;
        if (Physics.Raycast(ray, out hit, aimDistance, hitLayer))
        {
            aimAtTargetRotation = Quaternion.LookRotation(hit.point - shootStartTransform.position, Vector3.up);
        }
        else
        {
            Vector3 hitPosition = Camera.main.transform.position + Camera.main.transform.forward * aimDistance;
            aimAtTargetRotation = Quaternion.LookRotation(hitPosition - shootStartTransform.position, Vector3.up);
        }
        // Maybe TODO
        // I can change raycast to capsulecast instead, if crosshair is big and I want player to be able to aim more wide.

        impulseSource.GenerateImpulse();
        SoundManager.instance.PlaySoundOnce(shootSound, shootVolume);
        shootParticle.Play();
        
        Bullet bullet = Instantiate(shootObject, shootStartTransform.position, Quaternion.identity).GetComponent<Bullet>();
        bullet.SetBulletVelocity(aimAtTargetRotation);

    }

}
