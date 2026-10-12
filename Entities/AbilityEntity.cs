using System;
using Unity.Cinemachine;
using UnityEngine;

public class AbilityEntity : MonoBehaviour
{

    public event Action OnAbilityActivated;
    protected void CallOnAbilityActivated() => OnAbilityActivated?.Invoke();

    public event Action<string> OnAbilityAlerted;
    protected void CallAlertText(string message) => OnAbilityAlerted?.Invoke(message);

    [SerializeField] private Ability_SO abilityData;
    [SerializeField] private bool thirdPersonSideAim = false;
    //[SerializeField] private bool alignCameraAfterAim = false;

    [SerializeField] protected CinemachineImpulseSource impulseSource;
    [SerializeField] protected Transform shootStartTransform;
    [SerializeField] protected GameObject shootObject;
    [SerializeField] protected ParticleSystem shootParticle;
    [SerializeField] protected AudioClip shootSound;
    [SerializeField] protected float shootVolume = 1.0f;

    public Sprite GetAbilityIcon() => abilityData.icon;
    public AbilityType GetAbilityType() => abilityData.abilityType;
    public bool IsThirdPersonSideAim() => thirdPersonSideAim;
    //public bool ShouldAlignCameraAfterAim() => alignCameraAfterAim;

    public virtual void AbilityUpdate() {}

    public virtual void ActivateAbility() {}

    public virtual void AbilityUpdateEnded() {}

    public virtual void CancelAbility() {}

}