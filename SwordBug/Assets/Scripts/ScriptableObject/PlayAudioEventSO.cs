using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "Event/PlayAudioEventSO")]
public class PlayAudioEventSO : ScriptableObject
{
    public UnityAction<AudioClip> OnEventRaised;

    public void RaiseEvent(AudioClip clip)
    {
        OnEventRaised?.Invoke(clip);
    }
}
