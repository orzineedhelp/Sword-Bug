using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("ÊÂ¼þ¼àÌý")]
    public PlayAudioEventSO FXEvent;
    public PlayAudioEventSO BGMEvent;
    [Header("AudioSource")]
    public AudioSource FXSource;
    public AudioSource BGMSource;

    private void OnEnable()
    {
        FXEvent.OnEventRaised += OnFXEvent;
        BGMEvent.OnEventRaised += OnBGMEvent;
    }

    private void OnBGMEvent(AudioClip arg0)
    {
        BGMSource.clip = arg0;
        BGMSource.Play();
    }

    private void OnDisable()
    {
        FXEvent.OnEventRaised -= OnFXEvent;
        BGMEvent.OnEventRaised -= OnBGMEvent;

    }

    private void OnFXEvent(AudioClip arg0)
    {
        FXSource.clip=arg0;
        FXSource.Play();
    }
}
