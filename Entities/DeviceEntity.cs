using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class DeviceEntity : MonoBehaviour
{

    [Header("Device General Details")]
    protected ConnectionNode[] m_ConnectionNodes;
    protected SequenceController m_SequenceController;

    public enum DeviceState
    {
        UnActivated,
        InProcess,
        Activated
    }

    public DeviceState CurrentState { get; protected set; }

    [SerializeField] private DeviceType deviceType;
    [SerializeField] private bool playSequenceWhenActivated = false;

    protected virtual void Awake()
    {

        m_ConnectionNodes = GetComponentsInChildren<ConnectionNode>();

        foreach (var node in m_ConnectionNodes)
        {
            node.Init(this);
        }

        if (playSequenceWhenActivated)
        {
            m_SequenceController = GetComponentInChildren<SequenceController>();

            if (m_SequenceController != null)
                m_SequenceController.Init(OnSequenceFinishedCallback);
        }

    }

    public virtual void OnDeviceConnected() { }
    public virtual void OnDeviceDisconnected() { }

    protected virtual void ActivateDevice()
    {

        if (playSequenceWhenActivated)
        {
            if (m_SequenceController != null)
                m_SequenceController.PlaySequence();
            else
                Debug.LogWarning($"Sequence Controller has not been set.");
        }

    }

    public DeviceType GetDeviceType() => deviceType;

    protected virtual void OnSequenceFinishedCallback() {}

}
