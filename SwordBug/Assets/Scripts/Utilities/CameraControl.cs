using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System.Xml.Serialization;
using System;

public class CameraControl : MonoBehaviour
{
    [Header("ÊÂ¼þ¼àÌý")]
    public VoidEventSO afterLoadedEvent;
    private CinemachineConfiner2D cc2d;
    public CinemachineImpulseSource impulseSource;
    public VoidEventSO cameraShakeEvent;
    private void Awake()
    {
        cc2d=GetComponent<CinemachineConfiner2D>();

    }
    private void OnEnable()
    {
        cameraShakeEvent.OnEventRaised += OnCameraShakeEvent;
        afterLoadedEvent.OnEventRaised += OnAfterScenceLoadedEvent;
    }

    private void OnAfterScenceLoadedEvent()
    {
        GetNewCameraBounds();
    }

    private void OnDisable()
    {
        cameraShakeEvent.OnEventRaised -= OnCameraShakeEvent;
        afterLoadedEvent.OnEventRaised -= OnAfterScenceLoadedEvent;

    }

    private void OnCameraShakeEvent()
    {
      impulseSource.GenerateImpulse();
    }

    
    private void GetNewCameraBounds()
    {
        var obj = GameObject.FindGameObjectWithTag("Bounds");
        if (obj == null) return;
        cc2d.m_BoundingShape2D=obj.GetComponent<Collider2D>();
        cc2d.InvalidateCache();
    }
}
