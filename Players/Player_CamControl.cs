using System;
using System.Runtime.CompilerServices;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player_CamControl : PlayerComponentEntity
{

    public bool IsAiming { get; private set; }
    public Vector2 LookWhileAimVector { get; private set; }

    #region Internal Variables
    private CinemachineOrbitalFollow m_OrbitFollow;
    private CinemachineInputAxisController m_AxisController;

    private Player_Movement m_PlayerMovement;
    private Player_AbilityManager m_PlayerAbility;
    #endregion

    [Header("Object Details")]
    [SerializeField] private GameObject normalCam;
    [SerializeField] private GameObject aimingCam;
    [Space]
    [SerializeField] private UIController uiController;

    [Header("Aim Details")]
    [SerializeField] private Transform aimCamLookAtTransform;
    [SerializeField] private float lookVerticalSpeed = 0.02f;
    [SerializeField] private float lookHorizontalSpeed = 0.05f;
    [SerializeField] private Vector2 pitchAngleLimit;

    [Header("Effect Details")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip equipSound;
    [SerializeField] private AudioClip unequipSound;


    private void Awake()
    {

        m_PlayerMovement = GetComponent<Player_Movement>();
        m_PlayerAbility = GetComponent<Player_AbilityManager>();

        m_OrbitFollow = normalCam.GetComponent<CinemachineOrbitalFollow>();
        m_AxisController = normalCam.GetComponent<CinemachineInputAxisController>();

    }

    public override void Init(Player player, WorldHeart_InputAction inputAction)
    {
        base.Init(player, inputAction);

        IsAiming = false;

        m_InputAction.Player.Aim.performed += AimStarted;
        m_InputAction.Player.Aim.canceled += AimStopped;

        m_InputAction.Player.Look.performed += LookWhileAim;
        m_InputAction.Player.Look.canceled += LookWhileAim;

        CinemachineCore.CameraActivatedEvent.AddListener(OnCameraChanged);
    }

    private void OnDisable()
    {
        CinemachineCore.CameraActivatedEvent.RemoveListener(OnCameraChanged);

        m_InputAction.Player.Aim.performed -= AimStarted;
        m_InputAction.Player.Aim.canceled -= AimStopped;

        m_InputAction.Player.Look.performed -= LookWhileAim;
        m_InputAction.Player.Look.canceled -= LookWhileAim;
    }

    private void Update()
    {

        if (IsAiming == false)
            return;

        if (m_PlayerAbility.currentAbility.IsThirdPersonSideAim())
            AimThirdPersonCam();

    }

    //public ConnectionNode GetConnectablePoint() => m_CurrentConnetable;

    public void SetEnableAxisInput(bool enable) => m_AxisController.enabled = enable;

    private void LookWhileAim(InputAction.CallbackContext context)
    {

        Vector2 inputLookValue = context.ReadValue<Vector2>();
        inputLookValue.y *= -1;
        LookWhileAimVector = inputLookValue;

    }

    public void GetLookDirections(out Vector3 forward, out Vector3 right)
    {

        bool usingPulseAim = IsAiming && m_PlayerAbility.currentAbility.GetAbilityType() == AbilityType.PulseCanon;

        Transform lookSource = usingPulseAim ? aimCamLookAtTransform : Camera.main.transform;

        forward = lookSource.forward;
        right = lookSource.right;

    }

    private void AimStarted(InputAction.CallbackContext ctx)
    {

        IsAiming = true;

        PlaySoundEffect(true);

        if (m_PlayerAbility.currentAbility.IsThirdPersonSideAim())
            UpdatePlayerYawAndAimTargetTransform();

    }

    private void AimStopped(InputAction.CallbackContext ctx)
    {

        StopAiming();

    }

    public void StopAiming()
    {

        if (IsAiming == false)
            return;

        IsAiming = false;

        PlaySoundEffect(false);

        m_PlayerAbility.CurrentAbilityUpdatedEnded();

        if (normalCam.activeSelf == false /*m_PlayerAbility.currentAbility.ShouldAlignCameraAfterAim()*/)
        {
            // Set the camera's yaw and pitch to match the player's current rotation
            m_OrbitFollow.HorizontalAxis.Value = m_PlayerMovement.GetCharacterEular().y;

            float currentPitch = aimCamLookAtTransform.eulerAngles.x;

            if (currentPitch > 180)
                currentPitch -= 360f;

            m_OrbitFollow.VerticalAxis.Value = Mathf.Clamp(currentPitch, m_OrbitFollow.VerticalAxis.Range.x, m_OrbitFollow.VerticalAxis.Range.y);
            //----------------------------------------------------------------------
        }

        SwitchCameraTo(true);

    }

    private void PlaySoundEffect(bool equip)
    {

        audioSource.Stop();
        audioSource.clip = equip ? equipSound : unequipSound;
        audioSource.Play();

    }

    private void AimThirdPersonCam()
    {

        // Rotating camera yaw and pitch, based on mouse movement.
        float yawAngle = aimCamLookAtTransform.rotation.eulerAngles.y + LookWhileAimVector.x * /*Time.deltaTime * */lookHorizontalSpeed;

        float m_CurrentPitch = aimCamLookAtTransform.rotation.eulerAngles.x;
        if (m_CurrentPitch > 180)
            m_CurrentPitch -= 360f;

        float pitchAngle = m_CurrentPitch + LookWhileAimVector.y * /*Time.deltaTime * */lookVerticalSpeed;
        pitchAngle = Mathf.Clamp(pitchAngle, pitchAngleLimit.x, pitchAngleLimit.y);

        Vector3 angle = new Vector3(pitchAngle, yawAngle, 0);
        aimCamLookAtTransform.rotation = Quaternion.Euler(angle);
        //cameraLookAtTransform.forward = angle;

        // Rotating character along with where camera's pointing at.
        Vector3 characterForward = aimCamLookAtTransform.forward;
        characterForward.y = 0;

        m_PlayerMovement.SetCharacterTurnForward(characterForward);

    }

    private void UpdatePlayerYawAndAimTargetTransform()
    {

        // Rotate lookAtTarget and player to camera forward direction
        Vector3 camForward = Camera.main.transform.forward;
        camForward.y = 0;
        aimCamLookAtTransform.forward = camForward;
        m_PlayerMovement.SetCharacterTurnForward(camForward);

        // Set aim target rotation.x from -20 to 20 degrees, based on camera's pitch angle
        Vector3 aimTargetAngle = aimCamLookAtTransform.rotation.eulerAngles;
        float cameraAngle = Camera.main.transform.rotation.eulerAngles.x;

        if (cameraAngle > 180)
            cameraAngle -= 360f;

        float pitchAngle = Mathf.Clamp(cameraAngle, pitchAngleLimit.x, pitchAngleLimit.y);
        aimTargetAngle.x = pitchAngle;
        aimCamLookAtTransform.rotation = Quaternion.Euler(aimTargetAngle);

        // Switch to aiming third person camera
        SwitchCameraTo(false);

    }

    private void SwitchCameraTo(bool setToNormalCam)
    {

        aimingCam.SetActive(!setToNormalCam);
        normalCam.SetActive(setToNormalCam);

    }

    private void OnCameraChanged(ICinemachineCamera.ActivationEventParams evt)
    {

        bool isAimingCamera = evt.IncomingCamera == aimingCam.GetComponent<ICinemachineCamera>();
        uiController.SetActiveCrosshair(isAimingCamera);

    }

}
