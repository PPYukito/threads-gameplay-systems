using System;
using Unity.VisualScripting;
using UnityEngine;

public class ConnectionNode : MonoBehaviour, IConnectionPoint
{
    public bool IsConnected { get; protected set; }
    public DeviceEntity NodeDevice { get; private set; }

    [SerializeField] private Transform connectionPoint;

    [Header("Interact Details")]
    [SerializeField] private AudioClip onConnectSound;
    [SerializeField] private float onConnectedVolume = 10.0f;
    [SerializeField] private AudioClip onDisConnectSound;
    [SerializeField] private float onDisConnectedVolume = 10.0f;
    [SerializeField] private GameObject onConnectedParticle;

    [Header("Placeholder Details")]
    [SerializeField] private CableHeadPlaceholder cableHeadPlaceholder;

    public void Init(DeviceEntity device)
    {

        NodeDevice = device;

    }

    public void Connected()
    {
        IsConnected = true;
        NodeDevice.OnDeviceConnected();

        SoundManager.instance.PlaySoundOnce(onConnectSound, onConnectedVolume);
        Instantiate(onConnectedParticle, connectionPoint.position, transform.rotation);
    }

    public void Disconnected()
    {
        if (IsConnected == false)
            return;

        IsConnected = false;

        try
        {
            NodeDevice.OnDeviceDisconnected();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"Error disconnecting device: {e.Message}");
            //NodeDevice.OnDeviceDisconnected();
        }

        SoundManager.instance.PlaySoundOnce(onDisConnectSound, onDisConnectedVolume);
    }

    public Transform GetConnectionPoint() => connectionPoint;

    public bool CanbeConnected()
    {
        if (IsConnected == true)
            return false;

        if (NodeDevice.CurrentState == DeviceEntity.DeviceState.Activated || NodeDevice.CurrentState == DeviceEntity.DeviceState.InProcess)
            return false;

        return true;
    }

    public void ShowPlaceholder(bool inRange) => cableHeadPlaceholder.ShowPlaceholder(inRange);
    public void StopShowPlaceholder() => cableHeadPlaceholder.UnshowPlaceholder();

}
