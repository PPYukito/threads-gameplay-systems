using UnityEngine;

public class CableHeadPlaceholder : MonoBehaviour
{

    private Material m_PlaceholderMaterial;

    [SerializeField] private GameObject placeholderGO;
    [SerializeField] private Renderer placeholderRenderer;

    [Header("Color Details")]
    [ColorUsage(showAlpha: true, hdr: true)]
    [SerializeField] private Color inRangeColor;
    [ColorUsage(showAlpha: true, hdr: true)]
    [SerializeField] private Color outRangeColor;

    private void OnEnable()
    {
        m_PlaceholderMaterial = placeholderRenderer.material;
    }

    public void ShowPlaceholder(bool inRange)
    {
        placeholderGO.SetActive(true);

        // Set the color based on whether the placeholder is in range or out of range
        Color hdr = inRange ? inRangeColor : outRangeColor;
        m_PlaceholderMaterial.SetColor("_FresnelColor", hdr);
    }

    public void UnshowPlaceholder() => placeholderGO.SetActive(false);

}
