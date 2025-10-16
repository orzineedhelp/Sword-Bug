using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


public class PlayerControl : MonoBehaviour
{
    private PlayerInputControl inputActions;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private PhysicsCheck pc;
    private PlayerAnimation pa;
    private CapsuleCollider2D coll;

    [SerializeField] private float runspeed = 200f;
    private Vector2 moveDirection;
    [SerializeField] private float jumpForce=10f;
    [SerializeField] private float jumpForce2 = 15f;


    [SerializeField] private float doubleJumpCooldown = 0.3f; // 二段跳冷却时间
    private bool doubleJump; // 是否可以进行二段跳
    private bool canDoubleJump = true; // 二段跳是否可用
    private float lastJumpTime; // 上一次跳跃的时间
    [SerializeField] private float hurtForce;

    [Header("状态查看")]
    public bool isDoubleJump = false;
    public bool isHurt;
    public bool isDead;
    public bool isAttack;
    public bool isHold;//判断手里是否有东西

    [Header("物理材质")]
    public PhysicsMaterial2D normal;
    public PhysicsMaterial2D wall;


    private void Awake()
    {
        inputActions=new PlayerInputControl();
        rb=GetComponent<Rigidbody2D>();
        sr=GetComponent<SpriteRenderer>();
        pc=GetComponent<PhysicsCheck>();
        pa=GetComponent<PlayerAnimation>();
        coll=GetComponent<CapsuleCollider2D>();
        inputActions.GamePlay.Jump.started += Jump;//注册事件

        //攻击
        inputActions.GamePlay.Attack.started += PlayerAttack;
        //使用道具
        inputActions.GamePlay.Use.started += PlayerUse;
    }

   
    void Update()
    {
        ReadMovement();
        UpdateDoubleJumpStatus();
        CheckState();
    }
    private void FixedUpdate()
    {
        if(!isHurt&&!isAttack) Move();
    }
    private void OnEnable()
    {
        inputActions.Enable();
    }
    private void OnDisable()
    {
        inputActions.Disable();
    }
    private void UpdateDoubleJumpStatus()
    {
        // 如果在地面上，重置二段跳状态
        if (pc.isGround)
        {
            canDoubleJump = true;
            isDoubleJump = false;
        }

        // 检查二段跳冷却时间是否结束
        if (!canDoubleJump && Time.time - lastJumpTime >= doubleJumpCooldown)
        {
            canDoubleJump = true;
        }
    }

    private void ReadMovement()
    {
        moveDirection=inputActions.GamePlay.Move.ReadValue<Vector2>();//利用新输入系统获取按键赋予的向量
       
    }
    
   
    private void Jump(InputAction.CallbackContext context)
    {
        // 在地面上跳跃
        if (pc.isGround)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0); // 重置Y轴速度，确保跳跃高度一致
            rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
            doubleJump = true; // 在地面上进行一阶段跳跃，获得二段跳条件
            lastJumpTime = Time.time;
            canDoubleJump = true; // 重置二段跳可用状态
        }
        // 在空中且可以进行二段跳且二段跳可用
        else if (!pc.isGround && doubleJump && canDoubleJump && !isDoubleJump)
        {
            isDoubleJump = true;
            rb.velocity = new Vector2(rb.velocity.x, 0); // 重置Y轴速度，确保跳跃高度一致
            rb.AddForce(transform.up * jumpForce2, ForceMode2D.Impulse);
            doubleJump = false;
            canDoubleJump = false; // 禁用二段跳，直到冷却结束
            lastJumpTime = Time.time;

            // 启动协程重置二段跳执行状态
            StartCoroutine(ResetDoubleJumpFlag());
        }
    }

    private void PlayerAttack(InputAction.CallbackContext context)
    {
        // 只有手中持有物品时才能攻击
        if (!isHold)
        {
            Debug.Log("无法攻击：手中没有物品");
            return;
        }

        // 如果已经在攻击中，不允许再次攻击
        if (isAttack) return;

        pa.PlayAttack();
        isAttack = true;
    }

    public void PickUpItem()
    {
        isHold = true;
        Debug.Log("拾取物品，现在可以攻击和使用");
    }
    private void PlayerUse(InputAction.CallbackContext context)
    {
        // 只有手上拿着道具才可使用
        if (!isHold)
        {
            Debug.Log("无法使用：手中没有物品");
            return;
        }

        pa.UseThing();

        // 使用后物品消失，不能再使用
        isHold = false;
        Debug.Log("物品已使用，手中不再持有物品");

    }
    public void GetHurt(Transform attack)
    {
        isHurt = true;
        rb.velocity = Vector2.zero;
        Vector2 dir = new Vector2((transform.position.x - attack.position.x), 0).normalized;
        rb.AddForce(dir*hurtForce, ForceMode2D.Impulse);
    }
    private IEnumerator ResetDoubleJumpFlag()
    {
        yield return new WaitForSeconds(0.3f); // 短暂延迟后重置标志
        isDoubleJump = false;
    }
    public void Move()
    {
        rb.velocity = new Vector2(moveDirection.x * runspeed * Time.fixedDeltaTime, rb.velocity.y);
        if (moveDirection.x<0)
        {
            sr.flipX = true;
        }else if (moveDirection.x> 0) sr.flipX=false;
    }

    public void PlayDead()
    {
        isDead = true;
        inputActions.GamePlay.Disable();//玩家的所有操作取消
    }

    private void CheckState()
    {
        coll.sharedMaterial = pc.isGround ? normal : wall;
    }
    private void OnDestroy()
    {
        if (inputActions != null)
        {
            inputActions.GamePlay.Jump.started -= Jump;
            inputActions.GamePlay.Attack.started -= PlayerAttack;
            inputActions.GamePlay.Use.started -= PlayerUse;
        }
    }
}
