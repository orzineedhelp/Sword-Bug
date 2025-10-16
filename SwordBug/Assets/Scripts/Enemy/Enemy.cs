using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    Rigidbody2D rb;
    PhysicsCheck pc;
    SpriteRenderer sr;
    [Header ("基本参数")]
    [SerializeField] private float moveSpeed;
    public Vector3 faceDirec;
    [Header("撞墙计时器")]
    public float waitTime;
    public float waitCounter;
    public bool wait;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        pc = GetComponent<PhysicsCheck>();
        sr = GetComponent<SpriteRenderer>();
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
        Move();
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
}
