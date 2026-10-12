using UnityEngine;
using UnityEngine.UI;

public class Player_UI : MonoBehaviour
{
    private UIController m_UIController;

    public void UpdateAbilityIcon(Ability_SO abilityData)
    {
        m_UIController.SetAbilityImage(abilityData.icon);
    }

}
