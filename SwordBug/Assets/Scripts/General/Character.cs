using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Character : MonoBehaviour
{
    [Header("基本属性")]
    [SerializeField] private int maxHealth=6;
    [SerializeField] private int currentHealth;

    [Header("受伤无敌")]
    public float nohurtDuration;
    private float nohurtCounter;
    public bool nohurt;

    public UnityEvent<Transform> OnTakeDamage;//事件
    public UnityEvent OnDead;
    private void Start()
    {
        currentHealth = maxHealth;
    }
    private void Update()
    {
        if(nohurt)//如受伤，开始无敌时间倒计时
        {
            nohurtCounter -= Time.deltaTime;//计时器减去完成一帧的时间
            if (nohurtCounter <= 0)
            {
                nohurt = false;
            }
        }
    }
    public void TakeDamge(Attack attack)
    {
        if (nohurt) return;//如果是无敌时间内，在怪物触发盒中不受伤害
        if (currentHealth - attack.damage > 0)
        {
            currentHealth -= attack.damage;
            TriggerNoHurt();//进行受伤重置
            //受伤体现,利用事件加入方法
            OnTakeDamage?.Invoke(attack.transform);
        }
        else
        { 
            currentHealth = 0;
            //dead
            OnDead?.Invoke();
        }
    }

   private void TriggerNoHurt()
    {
        if (!nohurt)//如未受伤，则将计时器重置，准备下一次受伤倒计时
        {
            nohurt = true;
            nohurtCounter = nohurtDuration;
        }
    }
}
