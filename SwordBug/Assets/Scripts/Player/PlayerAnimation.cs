using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

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
        ani.SetBool("isPick", playerControl.isPick);
    }
    public void PlayHurt()
    {
        ani.SetTrigger("hurt");
    }

    public void PlayAttack()
    {
        ani.SetTrigger("attack");
        playerControl.ActiveArea(true);
    }
    public void SetAttackUI()
    {
        ani.SetTrigger("UI");

    }
   
    public void UseSword()
    {
        
        ani.SetTrigger("Sword");
       
    }
    public void UseBrick()
    {

        ani.SetTrigger("Brick");

    }
    
   

}
