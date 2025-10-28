using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Sign : MonoBehaviour
{
    public GameObject sprite;
    public bool canPress;
    private Animator animator;
    private PlayerInputControl playerInput;

    private void Awake()
    {
        animator = sprite.GetComponent <Animator>();
        playerInput = new PlayerInputControl();
        playerInput.Enable();
    }
    private void OnEnable()
    {
        InputSystem.onActionChange += onActionChange;
    }

    private void onActionChange(object obj, InputActionChange actionchange)
    {
        //if (actionchange == InputActionChange.ActionStarted) 
        //{
        //    var d=((InputAction)obj).activeControl.device;
        //    switch (d.device) {
        //        case Keyboard:
        //            animator.Play("keyboard");
        //            break;
        //        default:
        //            animator.Play();
        //            }
        //}
    }

    private void OnDisable()
    {
        
    }
    private void Update()
    {
        sprite.GetComponent<SpriteRenderer>().enabled=canPress;
       
    }
    public void ChangeSign(bool needUI)
    {
        canPress = needUI;
    }
    private void OnTriggerStay2D(Collider2D collision)
    {
        if ((collision.tag=="Sword"||collision.tag=="Phone"))
        {
            canPress = true;

        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Sword" || collision.tag == "Phone")
        {
            canPress = false;

        }
    }
}
