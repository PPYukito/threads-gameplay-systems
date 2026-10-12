using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Playables;

public class SequenceController : MonoBehaviour
{
    private event Action OnSequenceFinished;

    protected PlayableDirector m_PlayableDirector;

    private CinemachineCamera m_SequenceCamera;

    private void Awake()
    {
        m_PlayableDirector = GetComponent<PlayableDirector>();
        m_SequenceCamera = GetComponentInChildren<CinemachineCamera>(true);
    }

    //private void OnEnable()
    //{

    //    if (m_PlayableDirector != null)
    //    {
    //        m_PlayableDirector.played += OnSequencePlayed;
    //    }

    //}

    //private void OnDisable()
    //{

    //    if (m_PlayableDirector != null)
    //    {
    //        m_PlayableDirector.played -= OnSequencePlayed;
    //    }

    //}

    public void Init(Action onSequenceFinished)
    {
        OnSequenceFinished = onSequenceFinished;
    }

    public void PlaySequence() => m_PlayableDirector?.Play();

    public void OnSequencePlayed()
    {
        Debug.LogWarning($"Sequence started playing");

        GameplayManager.instance.SetEnablePlayerInput(false);
    }

    public void OnSequenceStopped()
    {
        Debug.LogWarning($"Sequence has stopped");

        GameplayManager.instance.SetEnablePlayerInput(true);

        // call back to destroy cable
        OnSequenceFinished?.Invoke();
    }

}
