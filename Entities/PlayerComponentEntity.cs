using UnityEngine;

public class PlayerComponentEntity : MonoBehaviour
{

    #region Inheritance Variables
    protected Player m_Player;
    protected WorldHeart_InputAction m_InputAction;
    #endregion

    public virtual void Init(Player player, WorldHeart_InputAction inputAction)
    {
        m_Player = player;
        m_InputAction = inputAction;
    }

}
