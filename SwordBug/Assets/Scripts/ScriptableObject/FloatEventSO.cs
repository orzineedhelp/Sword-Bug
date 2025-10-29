using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
[CreateAssetMenu(fileName = "Event/FloatEventSO")]

public class FloatEventSO : ScriptableObject
{
    public UnityAction<float> OnEventRaised;

    public void RaiseEvent(float amount)
    {
        OnEventRaised?.Invoke(amount);
    }
}
