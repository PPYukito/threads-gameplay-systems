using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Movement : PlayerComponentEntity
{
    #region Internal Variables
    private Player_CamControl m_PlayerAim;

    private CharacterController m_CharacterController;
    private Vector2 m_MoveVector;
    private Vector3 m_LastMoveDir;
    private float m_VerticalVelocity;
    #endregion

    [Header("Movement Details")]
    [SerializeField] private float normalMoveSpeed = 2.7f;
    [SerializeField] private float slowMoveSpeed = 1.5f;
    [Space]
    [SerializeField] private Transform modelParentTransform;
    [SerializeField] private float turnSpeed = 8.2f;
    [SerializeField] private float turnSpeedMultiplier = 100f;
    [Space]
    [SerializeField] private float jumpForce = 5.5f;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private float groundedStickForce = -2f;

    private void Awake()
    {

        m_PlayerAim = GetComponent<Player_CamControl>();
        m_CharacterController = GetComponent<CharacterController>();

    }

    public override void Init(Player player, WorldHeart_InputAction inputAction)
    {
        base.Init(player, inputAction);

        m_InputAction.Player.Move.performed += MovePerformed;
        m_InputAction.Player.Move.canceled += MoveCanceled;

        m_InputAction.Player.Jump.performed += Jump;
    }

    private void OnDisable()
    {
        if (m_InputAction == null)
            return;

        m_InputAction.Player.Move.performed -= MovePerformed;
        m_InputAction.Player.Move.canceled -= MoveCanceled;

        m_InputAction.Player.Jump.performed -= Jump;
    }

    private void Update()
    {
        Movement();
        Rotate();
    }

    private void MovePerformed(InputAction.CallbackContext ctx) => m_MoveVector = ctx.ReadValue<Vector2>();
    private void MoveCanceled(InputAction.CallbackContext ctx) => m_MoveVector = Vector2.zero;

    private void Jump(InputAction.CallbackContext ctx)
    {
        if (m_CharacterController.isGrounded == false)
            return;

        m_VerticalVelocity = jumpForce;
    }

    public Vector3 GetCharacterEular() => modelParentTransform.localEulerAngles;
    public Vector3 GetCharacterForward() => modelParentTransform.forward;
    public void SetCharacterTurnForward(Vector3 forward) => modelParentTransform.forward = forward;

    private void Movement()
    {

        m_PlayerAim.GetLookDirections(out Vector3 lookForward, out Vector3 lookRight);

        // Get camera forward and right vectors
        Vector3 camForward = Vector3.ProjectOnPlane(lookForward, Vector3.up).normalized;
        Vector3 camRight = Vector3.ProjectOnPlane(lookRight, Vector3.up).normalized;

        // Build world-space move direction from camera-relative input
        Vector3 moveDir = (camForward * m_MoveVector.y) + (camRight * m_MoveVector.x);
        moveDir.Normalize();
        m_LastMoveDir = moveDir;

        // CharacterController doesn't apply gravity on its own - track it manually.
        // Keep a small downward value while grounded so isGrounded stays reliable next frame.
        if (m_CharacterController.isGrounded && m_VerticalVelocity < 0f)
            m_VerticalVelocity = groundedStickForce;

        m_VerticalVelocity += gravity * Time.deltaTime;

        Vector3 horizontalMove = moveDir * (m_PlayerAim.IsAiming ? slowMoveSpeed : normalMoveSpeed);
        Vector3 fullMove = horizontalMove + Vector3.up * m_VerticalVelocity;

        m_CharacterController.Move(fullMove * Time.deltaTime);

    }

    private void Rotate()
    {

        if (m_PlayerAim.IsAiming == true)
            return;

        // Rotate the character to face the move direction
        if (m_LastMoveDir != Vector3.zero)
        {
            Quaternion rotateTo = Quaternion.LookRotation(m_LastMoveDir, Vector3.up);
            modelParentTransform.rotation = Quaternion.RotateTowards(modelParentTransform.rotation, rotateTo, turnSpeed * turnSpeedMultiplier * Time.deltaTime);
        }

    }

}
