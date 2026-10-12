using System.Collections;
using UnityEngine;

public class PoweringDevice : DeviceEntity, IPowerGenerator
{

    private bool m_IsStarted;
    private Rigidbody m_Rigidbody;
    private AudioSource m_AudioSource;
    private Material m_ReactMaterial;
    private float m_EmissionIntensity;
    private float m_ElapsedTime;
    private Color m_HdrColor;


    [Header("Jump Start Details")]
    [SerializeField] private bool needJumpStarted = false;
    [SerializeField] private ParticleSystem jumpStartParticle;
    [SerializeField] private Renderer reactMeshRenderer;
    [SerializeField] private float activateTime = 0.5f;
    [SerializeField] private float emissionIntensity = 5.0f;
    [SerializeField] private Color energyColor;


    [Header("Power Details")]
    [SerializeField] protected float supplyBandwidthAmount;


    protected override void Awake()
    {
        base.Awake();

        m_IsStarted = !needJumpStarted;
        m_Rigidbody = GetComponent<Rigidbody>();
        m_AudioSource = GetComponent<AudioSource>();

        m_ReactMaterial = reactMeshRenderer.material;
    }

    private void Start()
    {

        if (m_IsStarted)
            SetColorActivated();

    }

    public void JumpStart()
    {

        if (m_IsStarted == true)
            return;

        Debug.LogWarning("Jump Start");

        // Play the jump start particle effect and audio
        jumpStartParticle.Play();
        m_AudioSource.Play();

        StartCoroutine(StartActivate());

        m_IsStarted = true;

    }

    IEnumerator StartActivate()
    {

        m_ReactMaterial.EnableKeyword("_EMISSION");
        m_EmissionIntensity = 0;
        m_ElapsedTime = 0;

        while (m_ElapsedTime < activateTime)
        {
            m_ElapsedTime += Time.deltaTime;
            m_EmissionIntensity = Mathf.Lerp(0, emissionIntensity, m_ElapsedTime / activateTime);

            m_HdrColor = energyColor * Mathf.Pow(2f, m_EmissionIntensity);
            m_ReactMaterial.SetColor("_EmissionColor", m_HdrColor);

            yield return null;
        }

        SetColorActivated();
        //m_HdrColor = energyColor * Mathf.Pow(2f, emissionIntensity);
        //m_ReactMaterial.SetColor("_EmissionColor", m_HdrColor);

    }

    private void SetColorActivated()
    {

        m_ReactMaterial.EnableKeyword("_EMISSION");
        m_HdrColor = energyColor * Mathf.Pow(2f, emissionIntensity);
        m_ReactMaterial.SetColor("_EmissionColor", m_HdrColor);

    }

    public float GetPower()
    {

        if (m_IsStarted == false)
            return 0;

        return supplyBandwidthAmount;

    }

    public void SetKinematic(bool isKinematic) => m_Rigidbody.isKinematic = isKinematic;

}
