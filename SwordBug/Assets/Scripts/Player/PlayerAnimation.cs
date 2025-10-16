using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimation : MonoBehaviour
{
    private Animator ani;
    private Rigidbody2D rd;
    private PhysicsCheck pc;
    private PlayerControl playerControl;
    private Character character;

    private void Awake()
    {
        ani=GetComponent<Animator>();
        rd=GetComponent<Rigidbody2D>();
        pc=GetComponent<PhysicsCheck>();
        playerControl=GetComponent<PlayerControl>();
        character=GetComponent<Character>();
    }
    void Start()
    {
        
    }

    void Update()
    {
        SetAnimation();
    }
    public void SetAnimation()//实时更新状态
    {
        ani.SetFloat("velocityX", Mathf.Abs(rd.velocity.x));
        ani.SetFloat("velocityY", rd.velocity.y);
        ani.SetBool("isGround", pc.isGround);
        ani.SetBool("isDoubleJump", playerControl.isDoubleJump);
        ani.SetBool("isDead",playerControl.isDead);
        ani.SetBool("noHurt", character.nohurt);
        ani.SetBool("isAttack", playerControl.isAttack);
        ani.SetBool("isHold", playerControl.isHold);
    }
    public void PlayHurt()
    {
        ani.SetTrigger("hurt");
    }

    public void PlayAttack()
    {
        ani.SetTrigger("attack");
        
    }
    public void Pick()
    {
        //玩家拾取物品
    }
    public void UseThing()
    {
        //使用剑进行撑杆跳，或摆放方块
        //if (true)//剑
        //{
        //    ani.SetTrigger("Use");
        //}
        //else//方块
        //{
        //    ani.SetTrigger("Down");
        //}
    }
}
