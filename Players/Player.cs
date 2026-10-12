using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{

    private WorldHeart_InputAction m_InputAction;

    private Player_Movement m_PlayerMovement;
    private Player_AbilityManager m_PlayerAbility;
    private Player_CamControl m_PlayerCamControl;

    private Cable m_CurrentCable;
    private CableHead m_CurrentCableHead;


    private void Awake()
    {

        m_InputAction = new WorldHeart_InputAction();

        m_PlayerMovement = GetComponent<Player_Movement>();
        m_PlayerAbility = GetComponent<Player_AbilityManager>();
        m_PlayerCamControl = GetComponent<Player_CamControl>();

    }

    private void OnEnable()
    {

        m_InputAction.Enable();

        m_PlayerMovement.Init(this, m_InputAction);
        m_PlayerAbility.Init(this, m_InputAction);
        m_PlayerCamControl.Init(this, m_InputAction);

        m_InputAction.Player.Shoot.performed += Shoot;
        m_InputAction.Player.Interact.performed += Interact;

    }

    private void OnDisable()
    {

        m_InputAction.Disable();

        m_InputAction.Player.Shoot.performed -= Shoot;
        m_InputAction.Player.Interact.performed -= Interact;

    }

    private void Start()
    {
        CursorManager.instance.SetCursorState(CursorLockMode.Locked);
    }

    private void Update()
    {
        
        if (m_PlayerCamControl.IsAiming == true)
            m_PlayerAbility.UpdateCurrentAbility();

    }

    public void CancelCurrentAbility()
    {
        DetachCableCompletely();
    }

    private void Shoot(InputAction.CallbackContext ctx)
    {

        if (m_PlayerCamControl.IsAiming == false)
            return;

        m_PlayerAbility.ActivateCurrentAbility();

    }

    private void OnTriggerStay(Collider other)
    {

        if (other.CompareTag("CableHead") == false)
            return;

        if (m_CurrentCableHead != null)
            return;

        // Handle cable head interaction
        CableHead cableHead = other.GetComponent<CableHead>();
        if (cableHead == m_CurrentCableHead)
            return;

        m_CurrentCableHead = cableHead;

    }

    private void OnTriggerExit(Collider other)
    {

        if (other.CompareTag("CableHead") == false)
            return;

        if (m_CurrentCableHead != null)
        {
            m_CurrentCableHead = null;
            return;
        }

        CableHead cableHead = other.GetComponent<CableHead>();
        if (cableHead == m_CurrentCableHead)
            m_CurrentCableHead = null;

    }

    private void Interact(InputAction.CallbackContext context)
    {

        if (m_CurrentCableHead == null)
            return;

        m_CurrentCable = m_CurrentCableHead.GetComponentInParent<Cable>();

        m_CurrentCableHead.Detach();
        m_CurrentCableHead = null;

        // Get attach point, if there is no more cable head, destroy cable
        if (m_CurrentCable.GetAttachPointsAmount() == 0)
        {
            DestroyCable();
        }

    }

    private void DetachCableCompletely()
    {

        if (m_CurrentCable == null)
            return;

        m_CurrentCable.DetachAllNode();
        DestroyCable();

    }

    private void DestroyCable()
    {

        Destroy(m_CurrentCable.gameObject);
        m_CurrentCable = null;

    }

    //[ContextMenu("Toggle Input")]
    //public void ToggleInput()
    //{

    //    bool inputIsEnable = m_InputAction.Player.enabled;

    //    if (inputIsEnable)
    //        m_InputAction.Player.Disable();
    //    else
    //        m_InputAction.Player.Enable();

    //    m_PlayerCamControl.SetEnableAxisInput(!inputIsEnable);

    //}

    public void EnableInput()
    {

        m_InputAction.Player.Enable();
        m_PlayerCamControl.SetEnableAxisInput(true);

    }

    public void DisableInput()
    {

        m_InputAction.Player.Disable();
        m_PlayerCamControl.SetEnableAxisInput(false);

    }

}
