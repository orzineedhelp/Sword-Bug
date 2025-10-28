using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "Event/FadeEventSO")]

public class FadeEventSO : ScriptableObject
{
    public UnityAction<Color,float,bool> OnEventRaised;

    /// <summary>
    /// 画面逐渐变深
    /// </summary>
    /// <param name="duration"></param>
    public void FadeIn(float duration)
    {
        RaisedEvent(Color.black, duration,true);
    }

    /// <summary>
    /// 画面逐渐还原
    /// </summary>
    /// <param name="duration"></param>
    public void FadeOut(float duration)
    {
        RaisedEvent(Color.clear, duration, false);

    }

    public void RaisedEvent(Color target,float duration,bool fadein)
    {
        OnEventRaised?.Invoke(target,duration,fadein);
    }
   
}
