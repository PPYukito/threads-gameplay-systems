using Obi;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.VirtualTexturing;

public class Cable : MonoBehaviour
{

    private List<CableHead> m_CableHeads;
    private GameObject m_Player;
    private bool m_TransportEnergy;
    private bool m_CableEnergyIsDisplaying = false;
    private Material m_CableMaterial;
    private float m_CableMaterialOffset;
    private int m_EnergyFlowDirection = 1; // 1 for connected receive device first, -1 for conncted power source first

    private PoweringDevice m_Powering;
    private ReceiverDevice m_Receiver;

    [Header("Cable Line Details")]
    [SerializeField] private GameObject cableLine;
    [SerializeField] private MeshRenderer cableLineRenderer;
    [SerializeField] private Texture2D cableLineTexture;
    [SerializeField] private ObiRope rope;
    [SerializeField] private ObiSolver solver;
    [SerializeField] private ObiParticleAttachment[] endPointAttachments;
    [SerializeField] private ObiParticleAttachment[] connectionPointAttachments;
    //[SerializeField] private float connectionPointOffset = 0.35f;

    [Header("Visual Effect Details")]
    [SerializeField] private float energyFlowSpeed = 0.35f;


    private void OnEnable()
    {
        m_CableHeads = new List<CableHead>(GetComponentsInChildren<CableHead>());

        m_CableMaterial = cableLineRenderer.material;
    }

    private void Update()
    {

        TransportingEnergy();

    }

    private void OnDestroy()
    {
        if (m_Receiver != null)
        {
            m_Receiver.OnEnergyFullEvent -= HandleFinishTransferEnergy;
            m_Receiver.OnSequenceFinishedEvent -= HandleSequenceFinished;
        }

        DetachAllNode();
    }

    private void TransportingEnergy()
    {
        if (m_TransportEnergy == false)
            return;

        float powerAmount = m_Powering.GetPower();

        DisplayEnergyFlow(powerAmount);

        // Apply power to the receiver device;
        m_Receiver.ApplyPower(powerAmount);
    }

    public int GetAttachPointsAmount() => m_CableHeads.Count;

    public Vector3 GetFirstNodePosition()
    {
        return m_CableHeads[0].GetConnectionPointTransform().position;
    }

    private void DisplayEnergyFlow(float powerAmount)
    {
        if (powerAmount <= 0)
            return;

        if (m_CableEnergyIsDisplaying == false)
            SetDisplayCableEmission(true);

        // Update cable's material offset to simulate energy flow;
        m_CableMaterialOffset += powerAmount * energyFlowSpeed * m_EnergyFlowDirection * Time.deltaTime;
        m_CableMaterial.mainTextureOffset = new Vector2(1, m_CableMaterialOffset);
    }

    public void Init(GameObject player, GameObject cableHeadObject, ConnectionNode connectable)
    {
        m_Player = player;
        m_CableMaterialOffset = 0;

        SetNode(cableHeadObject, connectable);
    }

    public void SetNode(GameObject cableHeadObject, ConnectionNode connectable)
    {
        cableHeadObject.transform.SetParent(transform, false);

        CableHead cableHead = cableHeadObject.GetComponent<CableHead>();

        //Set the cable head's connection node and detach callback;
        cableHead.Init(DetachNode, connectable);

        // Add the cable head to the list of cable heads;
        m_CableHeads.Add(cableHead);

        CheckRenderCableLine();

        if (m_CableHeads.Count == 2)
            EmitConnectedDevices();
    }

    public void DetachNode(CableHead cableHead)
    {
        m_CableMaterialOffset = 0;

        ClearLink();

        m_CableHeads.Remove(cableHead);
        Destroy(cableHead.gameObject);

        CheckRenderCableLine();
    }

    public void DetachAllNode()
    {
        m_CableMaterialOffset = 0;
        m_TransportEnergy = false;

        foreach (var cableHead in m_CableHeads)
        {
            cableHead.DetachFromConnectionNode();
        }
    }

    private void CheckRenderCableLine()
    {
        // If there is less than 2 cable heads, disable the cable line;
        if (m_CableHeads.Count < 2)
        {
            cableLineRenderer.enabled = false;

            // Undisplay cable energy material emission;
            SetDisplayCableEmission(false);

            for (int i = 0; i < m_CableHeads.Count; i++)
            {
                endPointAttachments[i].enabled = false;
                connectionPointAttachments[i].enabled = false;

                endPointAttachments[i].target = null;
                connectionPointAttachments[i].target = null;
            }

            solver.transform.position = Vector3.zero;

            return;
        }

        // Set each node's transforms with cable connection and end transforms;
        for (int i = 0; i < m_CableHeads.Count; i++)
        {
            CableHead cableHead = m_CableHeads[i];

            // Points in rope is called "ParticlGroup", each has 3 indices at most, but we only need the first index to get the "attachment index;
            // Use "attachment index" to get the "solver index" to set the position of the rope's particle in the solver;
            int endPointAttachmentIndex = endPointAttachments[i].particleGroup.particleIndices[0];
            int endPointSolverIndex = rope.solverIndices[endPointAttachmentIndex];
            solver.positions[endPointSolverIndex] = cableHead.GetEndPointTransform().position;

            int connectionPointAttachmentIndex = connectionPointAttachments[i].particleGroup.particleIndices[0];
            int connectionPointSolverIndex = rope.solverIndices[connectionPointAttachmentIndex];
            solver.positions[connectionPointSolverIndex] = cableHead.GetConnectionPointTransform().position;

            // But doing above is not enough, because the solver will update the rope's particle positions in the next frame
            // so we need to set the attachment's target to the cable head's transforms;
            //--------------------------------------------------------------------------
            // Set the cable head's end and connection point transforms to the rope's attachment transforms;
            endPointAttachments[i].target = cableHead.GetEndPointTransform();
            connectionPointAttachments[i].target = cableHead.GetConnectionPointTransform();

            // Enable the attachments to make the rope follow the cable head's transforms;
            endPointAttachments[i].enabled = true;
            connectionPointAttachments[i].enabled = true;
        }

        DetermineEnergyFlowDirection();

        // Update the rope's position to at least the middle point between the two cable heads, so the rope will not be too springy;
        Vector3 firstCableHeadConnectionPoint = m_CableHeads[0].GetConnectionPointTransform().position;
        Vector3 secondCableHeadConnectionPoint = m_CableHeads[1].GetConnectionPointTransform().position;
        solver.transform.position = Vector3.Lerp(firstCableHeadConnectionPoint, secondCableHeadConnectionPoint, 0.5f);

        cableLineRenderer.enabled = true;
    }

    private void SetDisplayCableEmission(bool display)
    {
        if (display)
        {
            m_CableMaterial.SetTexture("_BaseMap", cableLineTexture);
            m_CableMaterial.EnableKeyword("_EMISSION");
        }
        else
        {
            m_CableMaterial.SetTexture("_BaseMap", null);
            m_CableMaterial.DisableKeyword("_EMISSION");
        }

        m_CableEnergyIsDisplaying = m_CableMaterial.IsKeywordEnabled("_EMISSION");
    }

    private void DetermineEnergyFlowDirection()
    {
        // Determine the energy flow direction based on the connected devices;
        if (m_CableHeads[0].connectable.NodeDevice.GetDeviceType() == DeviceType.PowerGenerator)
        {
            m_EnergyFlowDirection = -1; // Energy flow from power source to receiver
        }
        else
        {
            m_EnergyFlowDirection = 1; // Energy flow from receiver to power source
        }
    }

    private void EmitConnectedDevices()
    {
        foreach (CableHead cableHead in m_CableHeads)
        {
            ConnectionNode connectable = cableHead.connectable;

            // Check what type connectionNode is be connecting to;
            switch (connectable.NodeDevice.GetDeviceType())
            {
                case DeviceType.PowerGenerator:
                    m_Powering = connectable.NodeDevice.GetComponent<PoweringDevice>();
                    break;

                case DeviceType.PowerReceiver:
                    m_Receiver = connectable.NodeDevice.GetComponent<ReceiverDevice>();

                    m_Receiver.OnEnergyFullEvent -= HandleFinishTransferEnergy;
                    m_Receiver.OnEnergyFullEvent += HandleFinishTransferEnergy;

                    m_Receiver.OnSequenceFinishedEvent -= HandleSequenceFinished;
                    m_Receiver.OnSequenceFinishedEvent += HandleSequenceFinished;
                    break;
            }
        }

        // If both end has connect with power source and device, begin transport power;
        if (m_Powering != null && m_Receiver != null)
        {
            // Start transporting energy from the power source to the device;
            m_TransportEnergy = true;
        }
    }

    private void ClearLink()
    {
        m_TransportEnergy = false;

        if (m_Receiver != null)
        {
            m_Receiver.OnEnergyFullEvent -= HandleFinishTransferEnergy;
            m_Receiver.OnSequenceFinishedEvent -= HandleSequenceFinished;
        }

        m_Receiver = null;
        m_Powering = null;
    }

    private void HandleFinishTransferEnergy()
    {
        m_TransportEnergy = false;
        SetDisplayCableEmission(false);
    }

    public void HandleSequenceFinished() => Destroy(gameObject);

}
