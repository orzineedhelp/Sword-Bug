using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManeger : MonoBehaviour
{
    public PlayerHealth playerHealth;
    [Header("事件监听")]
    public CharacterEventSO HealthEvent;//由character利用事件传递信息，之后UIManeger接受并统一处理UI

    private void OnEnable()
    {
        HealthEvent.OnEventRaised += OnHealthEvent;//注册事件
    }
    private void OnDisable()
    {
        HealthEvent.OnEventRaised -= OnHealthEvent;//注销事件

    }

    private void OnHealthEvent(Character character)
    {
        //具体扣血UI显示逻辑执行
        int num = character.maxHealth- character.currentHealth;
        playerHealth.OnHealthChange(num,character.isheal);

    }
}
