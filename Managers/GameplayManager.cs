using UnityEngine;

public class GameplayManager : MonoBehaviour
{
    public static GameplayManager instance { get; private set; }

    [SerializeField] private Player player;
    [SerializeField] private UIController uiController;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetEnablePlayerInput(bool enable)
    {
        if (enable)
            player.EnableInput();
        else
            player.DisableInput();

        uiController.SetActiveUI(enable);
    }
}
