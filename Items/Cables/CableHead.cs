using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class CableHead : MonoBehaviour
{

    private enum CableHeadStatus
    {
        Moving,
        Connected,
    }

    private event Action<CableHead> OnCableHeadDetached;
    public ConnectionNode connectable { get; private set; }

    private CableHeadStatus cableHeadStatus;
    private Vector3 startPointTrasform;
    private Vector3 targetPointTransform;

    [Header("Movement Details")]
    [Range(0.1f, 0.3f)]
    [SerializeField] private float travelTime = 0.3f;

    [Header("Cable Transform Details")]
    [SerializeField] private Transform endPointTransform;
    [SerializeField] private Transform connectionPointTransform;

    public Transform GetEndPointTransform() => endPointTransform;
    public Transform GetConnectionPointTransform() => connectionPointTransform;

    // Desposal variables
    private float elapsedTime = 0f;
    //-----------------------------

    public void Init(Action<CableHead> detachCableCallback, ConnectionNode connectable)
    {
        elapsedTime = 0f;
        cableHeadStatus = CableHeadStatus.Moving;

        // Prepare connection node for connection and move to target;
        OnCableHeadDetached = detachCableCallback;
        this.connectable = connectable;
        startPointTrasform = transform.position;
        targetPointTransform = connectable.GetConnectionPoint().position;
    }

    public void Detach()
    {
        DetachFromConnectionNode();
        OnCableHeadDetached?.Invoke(this);
    }

    public void DetachFromConnectionNode()
    {
        if (connectable != null)
            connectable.Disconnected();
    }

    private void ReachedTargetPosition()
    {
        if (connectable == null)
            return;

        connectable.Connected();

        transform.position = targetPointTransform;
        transform.rotation = connectable.GetConnectionPoint().rotation;
    }

    private void Update()
    {
        if (cableHeadStatus == CableHeadStatus.Connected)
            return;

        // Moving to target position
        if (travelTime <= 0f) return;

        // Track time passed
        elapsedTime += Time.deltaTime;
        float progress = elapsedTime / travelTime;

        // Move smoothly from start to target in the specified time
        transform.position = Vector3.Lerp(startPointTrasform, targetPointTransform, progress);

        if (progress >= 1f)
        {
            cableHeadStatus = CableHeadStatus.Connected;

            ReachedTargetPosition();
        }
    }

}
