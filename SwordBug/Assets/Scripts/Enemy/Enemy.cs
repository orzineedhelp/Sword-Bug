using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    protected Rigidbody2D rb;
    protected PhysicsCheck pc;
    protected SpriteRenderer sr;
    private Animator ani;

    [Header ("移动速度")]
    [SerializeField] protected float moveSpeed;
    public Vector3 faceDirec;
    public Transform attacker;
    [Header("等待设置")]
    public float waitTime;
    public float waitCounter;
    public bool wait;
    [Header("受伤设置")]
    public bool isHurt;
    [SerializeField] private float hurtForce;

    [Header("FSM")]
    public bool isFSMControlled = false;

    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        pc = GetComponent<PhysicsCheck>();
        sr = GetComponent<SpriteRenderer>();
        ani = GetComponent<Animator>();
    }
    private void Update()
    {
        faceDirec = new Vector3(-transform.localScale.x,0,0);
        if(pc.touchLeftWall||pc.touchRightWall)
        {
            wait = true;
        }
        TimeCounter();
    }
    private void FixedUpdate()
    {
        if (!isFSMControlled && !wait && !isHurt)
        {
            Move();
        }
    }
    public virtual void Move()
    {
        rb.velocity=new Vector2(moveSpeed*faceDirec.x*Time.fixedDeltaTime,rb.velocity.y);
    }

    public void TimeCounter()
    {
        if (wait)
        {
            waitCounter-= Time.deltaTime;
            if(waitCounter <= 0)
            {
                wait=false;
                transform.localScale = new Vector3(faceDirec.x, transform.localScale.y, transform.localScale.z);
                waitCounter = waitTime;
            }
        }
    }

    public void OnTakeDamage(Transform attack)
    {
        attacker = attack;
        isHurt = true;
        ani.SetTrigger("hurt");
    }
}