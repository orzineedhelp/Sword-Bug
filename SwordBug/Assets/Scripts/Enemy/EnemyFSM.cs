using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum EnemyState { Patrol, Chase, Attack }

public class EnemyFSM : MonoBehaviour
{
    public EnemyState currentState = EnemyState.Patrol;

    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private PhysicsCheck pc;
    private Enemy enemy;

    [Header("Patrol")]
    public Transform[] waypoints;
    public float patrolSpeed = 1.5f;
    private int currentWaypointIndex = 0;

    [Header("Chase")]
    public float chaseSpeed = 3.5f;
    public float detectRange = 5f;
    public float loseRange = 8f;

    [Header("Wall Wait")]
    public float wallWaitTime = 0.5f;
    private float wallWaitCounter;
    private bool isWaitingAtWall;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
        pc = GetComponent<PhysicsCheck>();
        enemy = GetComponent<Enemy>();

        if (player == null)
            Debug.LogWarning("EnemyFSM: Player not found. Make sure Player has tag 'Player'.");
    }

    void Update()
    {
        if (player == null) return;
        if (enemy != null && enemy.isHurt) return;

        WallDetection();

        switch (currentState)
        {
            case EnemyState.Patrol:
                if (Vector3.Distance(transform.position, player.position) < detectRange)
                {
                    currentState = EnemyState.Chase;
                }
                break;

            case EnemyState.Chase:
                if (Vector3.Distance(transform.position, player.position) > loseRange)
                {
                    currentState = EnemyState.Patrol;
                }
                break;
        }
    }

    void FixedUpdate()
    {
        if (player == null) return;
        if (enemy != null && enemy.isHurt) return;
        if (isWaitingAtWall) return;

        switch (currentState)
        {
            case EnemyState.Patrol:
                PatrolMove();
                break;
            case EnemyState.Chase:
                ChaseMove();
                break;
        }
    }

    private void PatrolMove()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            rb.velocity = new Vector2(0, rb.velocity.y);
            return;
        }

        Transform targetWaypoint = waypoints[currentWaypointIndex];
        Vector2 direction = (targetWaypoint.position - transform.position).normalized;
        float distance = Vector2.Distance(transform.position, targetWaypoint.position);

        if (distance < 0.2f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }

        rb.velocity = new Vector2(direction.x * patrolSpeed, rb.velocity.y);
        FlipSprite(direction.x);
    }

    private void ChaseMove()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.velocity = new Vector2(direction.x * chaseSpeed, rb.velocity.y);
        FlipSprite(direction.x);
    }

    private void FlipSprite(float directionX)
    {
        if (directionX > 0.01f)
            sr.flipX = false;
        else if (directionX < -0.01f)
            sr.flipX = true;
    }

    private void WallDetection()
    {
        if (pc == null) return;

        if (pc.touchLeftWall || pc.touchRightWall)
        {
            if (!isWaitingAtWall)
            {
                isWaitingAtWall = true;
                wallWaitCounter = wallWaitTime;
                rb.velocity = new Vector2(0, rb.velocity.y);
            }
        }

        if (isWaitingAtWall)
        {
            wallWaitCounter -= Time.deltaTime;
            if (wallWaitCounter <= 0)
            {
                isWaitingAtWall = false;
                // 翻转 Sprite 并切换到下一个巡逻点
                sr.flipX = !sr.flipX;
                // 跳到下一个点（避免卡墙）
                if (waypoints.Length > 0)
                    currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            }
        }
    }

}