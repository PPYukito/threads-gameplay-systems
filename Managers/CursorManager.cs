using UnityEngine;

public class CursorManager : MonoBehaviour
{
    public static CursorManager instance;

    private void Awake()
    {
        MakeSingleton();
    }

    private void MakeSingleton()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
    }

    public void SetCursorState(/*bool isVisible, */CursorLockMode lockMode)
    {
        //Cursor.visible = isVisible;
        Cursor.lockState = lockMode;
    }
}
