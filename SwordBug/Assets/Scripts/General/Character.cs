using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Character : MonoBehaviour,ISaveable
{
    [Header("事件监听")]
    public VoidEventSO newGameEvent;
    [Header("组件")]
    public PhysicsCheck pc;
    public PlayerControl pcControl;
    public Rigidbody2D rb;
    public DialogueTrigger dialogue;
    public DialogueTrigger dialogueTetromino;
    public DialogueTrigger dialogueEnd;
    public AudioDefination audiodef;
    public PlayerAppearController appearController;
    [Header("基本属性")]
     public int maxHealth=6;
     public int currentHealth;
    private float originalScale;

    [Header("受伤无敌")]
    public float nohurtDuration;
    private float nohurtCounter;
    public bool nohurt;

    [Header("恢复间隔")]
    public float healDuration;
    public float healCounter;
    public bool isheal;
    [Header("是否死亡")]
    public bool isDead;
    public bool isRestart;

   

    public UnityEvent<Transform> OnTakeDamage;//事件
    public UnityEvent OnDead;

    public UnityEvent<Character> OnHealthChange;

    private void Awake()
    {
        pc = GetComponent<PhysicsCheck>();

        rb = GetComponent<Rigidbody2D>();
        originalScale = this.gameObject.transform.localScale.x;
        appearController = GetComponent<PlayerAppearController>();
    }
    private void NewGame()
    {
        currentHealth = maxHealth;
        if(appearController.enabled==false)
        { appearController.enabled = true; }
        
    }
    private void OnEnable()
    {
        newGameEvent.OnEventRaised += NewGame;
        ISaveable saveable = this;
        saveable.RegisterSaveData();
    }
    private void OnDisable()
    {
        newGameEvent.OnEventRaised -= NewGame;
        ISaveable saveable=this;
        saveable.UnRegistSaveData();
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
    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == "Spike")
        {
            TakeDamge(collision.GetComponent<Attack>());

        }
        
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Mushroom")
        {
            pcControl.ToggleInputSystem();
            Destroy(collision.gameObject);
            dialogue.ChangeisDialogue();
            pcControl.isMushroom = true;

        }
        if(collision.tag == "Spring")
        {
            collision.GetComponent<Animator>().SetTrigger("Jump");
            rb.velocity = new Vector2(rb.velocity.x, 0); // 重置Y轴速度
            rb.AddForce(transform.up * 25f, ForceMode2D.Impulse);
        }
        if (collision.tag =="Wood"&&transform.localScale==pcControl.originalScale)
        {
            collision.GetComponent<Animator>().SetTrigger("Broke");
            rb.velocity = new Vector2(rb.velocity.x, 0); // 重置Y轴速度
            rb.AddForce(transform.up * 15f, ForceMode2D.Impulse);
            //jump音效
            pcControl.audioJump.PlayAudioClip();
        }
        
        if( collision.tag == "Tetromino")
        {
            dialogueTetromino.ChangeisDialogue();
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Wood" && transform.localScale == pcControl.originalScale)
        {
            Destroy(collision.gameObject);
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

            isDead = true;
            //dead
            OnDead?.Invoke();
        }
        OnHealthChange?.Invoke(this);
    }
    public void AudioDead()
    {
        audiodef.PlayAudioClip();
    }
    public void TakeDamge()
    {
        currentHealth = 0;

        isDead = true;
            //dead
            OnDead?.Invoke();

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
           // healAudio.PlayAudioClip();
            healCounter = healDuration; // 重置冷却计时器
            OnHealthChange?.Invoke(this);
            //Debug.Log("恢复半格生命值，当前生命值: "+currentHealth);

        }
           
    }

    public DataDefinition GetDataID()
    {
        return GetComponent<DataDefinition>();
    }

    public void GetSaveData(Data data)
    {
        if (data.characterPosDict.ContainsKey(GetDataID().ID))
        {
            data.characterPosDict[GetDataID().ID]=transform.position;
            data.floatSavedData[GetDataID().ID + "health"] = maxHealth;//满血
            data.floatSavedData[GetDataID().ID + "scale.x"]=originalScale;//将初始大小赋值给data，
            data.boolSaveData[GetDataID().ID] = true;

        }
        else
        {
            data.characterPosDict.Add(GetDataID().ID, transform.position);
            Debug.Log("加入列表"+data.characterPosDict[GetDataID().ID]);

            data.floatSavedData.Add(GetDataID().ID + "health", maxHealth);//重启则满血
            data.floatSavedData.Add(GetDataID().ID + "scale.x", originalScale);//将初始大小赋值给data，以达到重启关卡后直接进入刚吃蘑菇状态
            data.boolSaveData.Add(GetDataID().ID,true);

        }
    }

    public void LoadData(Data data)
    {
        if (data.characterPosDict.ContainsKey(GetDataID().ID))
        {
            transform.position = data.characterPosDict[GetDataID().ID];
            this.currentHealth = (int)data.floatSavedData[GetDataID().ID + "health"];
            float scaleNum = data.floatSavedData[GetDataID().ID + "scale.x"];
            this.transform.localScale = new Vector3( scaleNum, scaleNum, scaleNum);
            isRestart = data.boolSaveData[GetDataID().ID];
            isDead = false;
            //通知血量
            Debug.Log("恢复血量" + (int)data.floatSavedData[GetDataID().ID + "health"]);
            OnHealthChange?.Invoke(this);
        }

    }
}
