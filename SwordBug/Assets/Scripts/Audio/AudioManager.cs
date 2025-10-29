using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [Header("ÊÂ¼þ¼àÌý")]
    public PlayAudioEventSO FXEvent;
    public PlayAudioEventSO BGMEvent;
    public FloatEventSO volumeEvent;
    public FloatEventSO BGMvolumeEvent;
    public FloatEventSO FXvolumeEvent;

    public VoidEventSO pauseEvent;
    [Header("¹ã²¥")]
    public FloatEventSO syncVolueEvent;
    public FloatEventSO syncVolueEventBGM;
    public FloatEventSO syncVolueEventFX;

    [Header("AudioSource")]
    public AudioSource FXSource;
    public AudioSource BGMSource;
    public AudioMixer audioMixer;
    private void OnEnable()
    {
        FXEvent.OnEventRaised += OnFXEvent;
        BGMEvent.OnEventRaised += OnBGMEvent;
        volumeEvent.OnEventRaised += OnVolumeEvent;
        BGMvolumeEvent.OnEventRaised += OnVolumeEvent;
        FXvolumeEvent.OnEventRaised += OnVolumeEvent;

        pauseEvent.OnEventRaised += OnPauseEvent;
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
        volumeEvent.OnEventRaised -= OnVolumeEvent;
        BGMvolumeEvent.OnEventRaised -= OnBGMVolumeEvent;
        FXvolumeEvent.OnEventRaised -= OnFXVolumeEvent;

        pauseEvent.OnEventRaised -= OnPauseEvent;

    }

    private void OnPauseEvent()
    {
        float amount_M;
        float amount_B;
        float amount_F;
        audioMixer.GetFloat("MasterVolume",out amount_M);
        audioMixer.GetFloat("BGMVolume", out amount_B);
        audioMixer.GetFloat("FXVolume", out amount_F);

        syncVolueEvent.RaiseEvent(amount_M);
        syncVolueEventBGM.RaiseEvent( amount_B);
        syncVolueEventFX.RaiseEvent( amount_F);
    }

    private void OnVolumeEvent(float amount_M)
    {
        audioMixer.SetFloat("MasterVolume",amount_M*100-80);
      
    }
    private void OnBGMVolumeEvent(float amount_B)
    {
      
        audioMixer.SetFloat("BGMVolume", amount_B * 100 - 80);
    }
    private void OnFXVolumeEvent(float amount_F)
    {
        audioMixer.SetFloat("FXVolume", amount_F * 100 - 80);
    }
    private void OnFXEvent(AudioClip arg0)
    {
        FXSource.clip=arg0;
        FXSource.Play();
    }
}
