using System;
using UnityEngine;
using UnityEngine.Playables;

public class ReceiverDevice : DeviceEntity, IPowerReceiver
{

    public bool CanBeAppliedPower { get; private set; }

    private float m_CurrentEnergyAmount = 0f;

    public event Action OnEnergyFullEvent;
    public event Action OnSequenceFinishedEvent;

    [Header("Receiver Device Details")]
    [SerializeField] private float energyAmountNeed = 20.0f;
    [SerializeField] private AudioSource receiverAudiouSource;
    [SerializeField] private AudioClip transferFinishedSoundClip;
    [SerializeField] private float transferFinishedSoundVolume = 0.35f;


    protected override void Awake()
    {
        base.Awake();

        CurrentState = DeviceState.UnActivated;
    }

    public override void OnDeviceConnected()
    {
        if (AllNodesConnected())
            NodeFullConnected();
    }

    private bool AllNodesConnected()
    {
        foreach (ConnectionNode node in m_ConnectionNodes)
        {
            if (node.IsConnected == false)
                return false;
        }

        return true;
    }

    public override void OnDeviceDisconnected()
    {

        CanBeAppliedPower = false;

        if (CurrentState == DeviceState.InProcess)
        {
            PlayEnergyChargeSound(false);
            CurrentState = DeviceState.UnActivated;
        }

    }

    protected virtual void NodeFullConnected() => CanBeAppliedPower = true;

    public void ApplyPower(float receiveAmount)
    {

        if (CurrentState == DeviceState.Activated)
            return;

        if (CanBeAppliedPower == false)
            return;

        ReceivePower(receiveAmount);

    }

    private void ReceivePower(float receiveAmount)
    {
        m_CurrentEnergyAmount += (receiveAmount * Time.deltaTime);

        // Play sound when the device is receiving energy and change state to InProcess
        if (m_CurrentEnergyAmount > 0 && CurrentState == DeviceState.UnActivated)
        {
            PlayEnergyChargeSound(true);
            CurrentState = DeviceState.InProcess;
        }

        // Check if the device has received enough energy to be activated
        if (m_CurrentEnergyAmount >= energyAmountNeed)
            OnEnergyFull();
    }

    private void OnEnergyFull()
    {
        SoundManager.instance.PlaySoundOnce(transferFinishedSoundClip, transferFinishedSoundVolume);
        PlayEnergyChargeSound(false);

        ActivateDevice();

        CurrentState = DeviceState.Activated;

        OnEnergyFullEvent?.Invoke();

        CanBeAppliedPower = false;
    }

    protected override void OnSequenceFinishedCallback()
    {
        OnSequenceFinishedEvent?.Invoke();
    }

    private void PlayEnergyChargeSound(bool play)
    {
        receiverAudiouSource.time = 0;

        if (play)
            receiverAudiouSource.Play();
        else
            receiverAudiouSource.Stop();
    }

}
