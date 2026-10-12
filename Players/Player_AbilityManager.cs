using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player_AbilityManager : PlayerComponentEntity
{
    public AbilityEntity currentAbility { get; private set; }

    #region Internal Variables
    private Player_CamControl m_PlayerCamcontrol;
    private int m_CurrentAbilityIndex = 0;
    #endregion

    [Header("Ability Details")]
    [SerializeField] private UIController uiController;
    [SerializeField] private List<AbilityEntity> abilities;


    private void Awake()
    {

        m_PlayerCamcontrol = GetComponent<Player_CamControl>();

    }

    public override void Init(Player player, WorldHeart_InputAction inputAction)
    {
        base.Init(player, inputAction);

        m_InputAction.Player.ChangeAbility.performed += ScrollHandler;

        // Setup callback after use ability
        foreach (AbilityEntity ability in abilities)
        {
            ability.OnAbilityActivated += AbilityActived;
            ability.OnAbilityAlerted += AlertText;
        }

        SetCurrentAbility();
        UpdateAbilityUI(); ;
    }

    private void OnDisable()
    {
        // Release callback after use ability
        foreach (AbilityEntity ability in abilities)
        {
            ability.OnAbilityActivated -= AbilityActived;
            ability.OnAbilityAlerted -= AlertText;
        }

        if (m_InputAction == null)
            return;
        m_InputAction.Player.ChangeAbility.performed -= ScrollHandler;
    }


    public void UpdateCurrentAbility() => currentAbility.AbilityUpdate();

    public void CurrentAbilityUpdatedEnded() => currentAbility.AbilityUpdateEnded();

    public void ActivateCurrentAbility() => currentAbility.ActivateAbility();

    private void SetCurrentAbility() =>currentAbility = abilities[m_CurrentAbilityIndex];

    private void UpdateAbilityUI() => uiController.SetAbilityImage(currentAbility.GetAbilityIcon());

    private void AbilityActived()
    {

        m_PlayerCamcontrol.StopAiming();

    }

    private void AlertText(string message)
    {
        uiController.ShowAlertText();
    }

    private void ScrollHandler(InputAction.CallbackContext ctx)
    {

        if (m_PlayerCamcontrol.IsAiming == true)
            return;

        #region MyOld Changing Method
        //Vector2 scrollValue = ctx.ReadValue<Vector2>();
        //scrollValue.Normalize();

        //if (scrollValue.y == 0)
        //    return;

        //ChangeAbility((int)scrollValue.y);
        #endregion

        #region New Changing Method
        Vector2 scrollValue = ctx.ReadValue<Vector2>();
        float y = scrollValue.y;

        // Ignore noise
        if (Mathf.Abs(y) < 0.01f)
            return;

        currentAbility.CancelAbility();

        int direction = y > 0 ? 1 : -1;

        ChangeAbility(direction);
        #endregion

    }

    private void ChangeAbility(int direction)
    {
        if (abilities == null || abilities.Count == 0)
            return;

        m_CurrentAbilityIndex = (m_CurrentAbilityIndex + direction + abilities.Count) % abilities.Count;

        //add changing ability sound;
        //make more feed back for changing ability

        SetCurrentAbility();
        UpdateAbilityUI();
    }

    //private void ChangeAbility(int yValue)
    //{

    //    m_CurrentAbility += yValue;

    //    if (m_CurrentAbility >= Ability.Max)
    //        m_CurrentAbility = 0;
    //    else if (m_CurrentAbility < 0)
    //        m_CurrentAbility = Ability.Max - 1;

    //    SetAbilityIcon(m_CurrentAbility);

    //}

}
