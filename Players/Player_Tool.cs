using UnityEngine;
using UnityEngine.InputSystem;

public class Player_Tool : PlayerComponentEntity
{
    private Player_AbilityManager m_PlayerAbility;
    private Player_CamControl m_PlayerAim;

    private void Awake()
    {
        m_PlayerAbility = GetComponent<Player_AbilityManager>();
        m_PlayerAim = GetComponent<Player_CamControl>();
    }

    public override void Init(Player player, WorldHeart_InputAction inputAction)
    {

        base.Init(player, inputAction);

        m_InputAction.Player.Shoot.performed += Shoot;

    }

    public void Shoot(InputAction.CallbackContext ctx)
    {
        if (m_PlayerAim.IsAiming == false)
            return;

        switch(m_PlayerAbility.currentAbility.GetAbilityType())
        {
            case AbilityType.PulseCanon:
                break;
            case AbilityType.EnergyCable:
                break;
        }
    }

}
