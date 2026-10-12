using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
    private Coroutine m_AlertCoroutine;

    [SerializeField] private GameObject uiParent;
    [Space]
    [SerializeField] private Image abilityImage;
    [SerializeField] private GameObject crosshairGameObject;

    [Header("Alert Details")]
    [SerializeField] private GameObject outOfRangeAlertText;
    [SerializeField] private float alertTextDuration = 2.0f;

    private void OnEnable()
    {
        SetActiveAlertOutOfRange(false);
    }

    private void OnDisable()
    {
        SetActiveAlertOutOfRange(false);

        m_AlertCoroutine = null;
    }

    public void SetActiveUI(bool active)
    {
        uiParent.SetActive(active);
    }

    public void SetAbilityImage(Sprite abilitySprite)
    {
        abilityImage.sprite = abilitySprite;
    }

    public void SetActiveCrosshair(bool active)
    {
        crosshairGameObject.SetActive(active);
    }

    public void ShowAlertText()
    {
        if (m_AlertCoroutine != null)
            StopCoroutine(m_AlertCoroutine);

        SetActiveAlertOutOfRange(true);

        m_AlertCoroutine = StartCoroutine(CloseAlertCo());
    }

    IEnumerator CloseAlertCo()
    {
        yield return new WaitForSeconds(alertTextDuration);
        SetActiveAlertOutOfRange(false);
    }

    private void SetActiveAlertOutOfRange(bool active)
    {
        outOfRangeAlertText.SetActive(active);
    }
}
