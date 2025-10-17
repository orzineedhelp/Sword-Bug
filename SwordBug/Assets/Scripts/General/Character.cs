using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Character : MonoBehaviour
{
    public PhysicsCheck pc;
    [Header("基本属性")]
     public int maxHealth=6;
     public int currentHealth;

    [Header("受伤无敌")]
    public float nohurtDuration;
    private float nohurtCounter;
    public bool nohurt;

    [Header("恢复间隔")]
    public float healDuration;
    public float healCounter;
    public bool isheal;

    public UnityEvent<Transform> OnTakeDamage;//事件
    public UnityEvent OnDead;

    public UnityEvent<Character> OnHealthChange;

    private void Awake()
    {
        pc = GetComponent<PhysicsCheck>();
    }
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
        HealBlood();
        if (isheal)
        {
            healCounter-= Time.deltaTime;
            if (healCounter <= 0)
            {
                isheal = false;
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
        OnHealthChange?.Invoke(this);
    }

   private void TriggerNoHurt()
    {
        if (!nohurt)//如未受伤，则将计时器重置，准备下一次受伤倒计时
        {
            nohurt = true;
            nohurtCounter = nohurtDuration;
        }
    }
    public void HealBlood()
    {
        if (isheal)
        {
            healCounter -= Time.deltaTime;
            if (healCounter <= 0)
            {
                isheal = false;
            }
        }
        if (!isheal&&pc.isCelling && currentHealth < maxHealth)
        {
            currentHealth++;
            isheal = true;
            healCounter = healDuration; // 重置冷却计时器
            OnHealthChange?.Invoke(this);
            //Debug.Log("恢复半格生命值，当前生命值: "+currentHealth);

        }
           
    }
}
