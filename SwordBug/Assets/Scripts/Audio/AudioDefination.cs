using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioDefination : MonoBehaviour
{
    public PlayAudioEventSO playAudioEvent;
    public AudioClip clip;
    public bool playOnEnable;
    private void OnEnable()
    {
        if (playOnEnable) PlayAudioClip();
    }
    private void OnDisable()
    {
        
    }
    public void PlayAudioClip()
    {
       
        playAudioEvent.OnEventRaised(clip);
    }
}
