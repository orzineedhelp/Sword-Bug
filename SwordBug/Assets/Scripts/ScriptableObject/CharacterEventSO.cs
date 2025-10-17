using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "Event/CharacterEventSO")]
public class CharacterEventSO : ScriptableObject
{
    public UnityAction<Character> OnEventRaised;//传递事件

    //事件订阅
    public void RaiseEvent(Character character)
    {
        OnEventRaised?.Invoke(character);
    }
}
