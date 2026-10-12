using UnityEngine;

public class Ability_EnergyCable : AbilityEntity
{

    private Cable m_CurrentCable;
    private ConnectionNode m_CurrentConnetable;
    private bool m_TargetInRange;


    [Header("Aim Details")]
    [SerializeField] private Transform aimCableTransform;
    [SerializeField] private float aimCableDistance = 5.0f;
    [SerializeField] private float aimMaxDistance = 30.0f;
    [SerializeField] private float aimDotThreshold = 0.65f;
    [SerializeField] private LayerMask cableShootPointLayer;

    [Header("Shoot Details")]
    [SerializeField] private GameObject cableGameObject;


    public override void AbilityUpdate() // Aim Cable
    {

        ConnectionNode target = FindAimTarget();

        if (target == null)
        {
            EndAimingPlaceHolder();
            return;
        }

        // Still aiming at the same node (or still at nothing), nothing to change
        if (target == m_CurrentConnetable)
            return;

        // Target changed: remove the old placeholder and clear the old target
        EndAimingPlaceHolder();

        m_CurrentConnetable = target;
        m_TargetInRange = m_CurrentCable == null || CheckRange();
        m_CurrentConnetable.ShowPlaceholder(m_TargetInRange);

    }

    private bool CheckRange()
    {
        float distance = Vector3.Distance(m_CurrentCable.GetFirstNodePosition(), m_CurrentConnetable.GetConnectionPoint().position);
        if (distance > aimMaxDistance)
            return false;

        return true;
    }

    // Returns the node the player is aiming at, only if it can really be connected right now
    private ConnectionNode FindAimTarget()
    {

        if (Physics.Raycast(aimCableTransform.position, aimCableTransform.forward, out RaycastHit hit, aimCableDistance, cableShootPointLayer) == false)
            return null;

        ConnectionNode connectable = hit.collider.GetComponent<ConnectionNode>();

        if (connectable == null || connectable.CanbeConnected() == false)
            return null;

        float dotProduct = Vector3.Dot(connectable.GetConnectionPoint().forward, -aimCableTransform.forward);
        if (dotProduct < aimDotThreshold)
            return null;

        return connectable;

    }

    public override void ActivateAbility() // Shoot Cable
    {

        if (m_CurrentConnetable == null)
            return;

        // Check if the current connectable object is out of range;
        if (m_TargetInRange == false)
        {
            // Alert player that the target is out of range
            CallAlertText("");
            return;
        }

        ConnectionNode connectable = m_CurrentConnetable;

        if (connectable.CanbeConnected() == false)
            return;

        Transform connectPoint = connectable.GetConnectionPoint();

        // Spawn CableHead at the connection point of the connectable object
        // Cablehead will fly to the targetobject ,
        // and when it reach the target, it will call the Connected() function of the connectable object.
        //GameObject node = Instantiate(tempCableHead, connectPoint.position, connectPoint.rotation);
        GameObject node = Instantiate(shootObject, shootStartTransform.position, Quaternion.identity);

        if (m_CurrentCable == null)
        {
            m_CurrentCable = Instantiate(cableGameObject).GetComponent<Cable>();
            m_CurrentCable.Init(gameObject, node, connectable);
        }
        else
        {
            m_CurrentCable.SetNode(node, connectable);

            // Shoot both connection, so player don't have to get hold of calbe anymore.
            m_CurrentCable = null;
        }

        // Shake camera
        impulseSource.GenerateImpulse();

        // Play shoot sound
        SoundManager.instance.PlaySoundOnce(shootSound, shootVolume);

        // Play shoot particle
        shootParticle.Play();

        // Stop aiming
        CallOnAbilityActivated();

    }

    public override void AbilityUpdateEnded() => EndAimingPlaceHolder();

    public void EndAimingPlaceHolder()
    {

        if (m_CurrentConnetable == null)
            return;

        m_TargetInRange = false;

        m_CurrentConnetable.StopShowPlaceholder();

        m_CurrentConnetable = null;

    }

    public override void CancelAbility()
    {

        if (m_CurrentCable == null)
            return;

        m_CurrentCable.DetachAllNode();

        // Destroy cable gameobject and reset current cable reference
        Destroy(m_CurrentCable.gameObject);
        m_CurrentCable = null;

    }

}
