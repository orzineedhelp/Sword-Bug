using UnityEngine;
using System.Collections;

/// <summary>
/// 玩家出现控制器
/// 控制玩家的登场动画、移动和组件激活流程
/// </summary>
public class PlayerAppearController : MonoBehaviour
{
    // ========== 移动设置 ==========
    [Header("移动设置")]
    [Tooltip("目标物体，玩家将移动到这个 GameObject 的位置")]
    public GameObject targetObject;

    [Tooltip("移动速度（单位/秒）")]
    public float moveSpeed = 5f;

    [Tooltip("旋转速度，控制玩家朝向目标的速度")]
    public float rotationSpeed = 2f;

    [Tooltip("停止距离，当与目标的距离小于此值时停止移动")]
    public float stoppingDistance = 0.1f;

    [Tooltip("延迟时间，游戏开始后等待多久开始移动和激活玩家")]
    public float delayTime = 2f;

    // ========== 旋转效果设置 ==========
    [Header("旋转效果设置")]
    [Tooltip("是否启用旋转效果，启用时玩家会在移动过程中旋转")]
    public bool enableSpinEffect = true;

    [Tooltip("旋转速度（度/秒）")]
    public float spinSpeed = 360f;

    [Tooltip("旋转动画曲线，控制旋转速度在移动过程中的变化")]
    public AnimationCurve spinCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

   
    // ========== 高级选项 ==========
    [Header("高级选项")]
    [Tooltip("是否使用平滑旋转过渡（否则直接设置旋转）")]
    public bool useSmoothRotation = true;

    [Tooltip("是否在控制台显示调试信息")]
    public bool showDebugInfo = true;

    // ========== 私有组件引用 ==========
    private Animator playerAnimator;           // 玩家的动画控制器组件
    private SpriteRenderer playerSpriteRenderer; // 玩家的精灵渲染器组件
    private Rigidbody2D playerRigidbody;       // 玩家的刚体2D组件
    private PlayerControl playerControl;       // 玩家控制脚本组件

    public GoldDialogueTrigger dialogueTrigger;
    public GameObject lightFlash;
    // ========== 状态变量 ==========
    private bool isMoving = false;             // 是否正在移动中
    private bool hasArrived = false;           // 是否已到达目标位置
    private bool isActivated = false;          // 是否已完成延迟激活
    public Vector3 initialPosition;           // 玩家的初始位置
    private Quaternion initialRotation;        // 玩家的初始旋转



    private void OnEnable()
    {
        // 当组件启用时，开始初始化流程
        if (!isActivated)
        {
            StartCoroutine(DelayedStart());
        }
    }

    private IEnumerator DelayedStart()
    {
        // 等待一帧确保所有组件已加载
        yield return null;

        // 调用初始化
        Start();
    }
    /// <summary>
    /// 初始化方法 - 在游戏开始时调用
    /// 设置初始状态，获取组件引用，准备移动
    /// </summary>
    void Start()
    {
        Debug.Log($"PlayerAppearController.Start() 被调用 - 激活状态: {isActivated}");
        if (isActivated) return; // 防止重复初始化
        // 保存初始位置和旋转，用于后续重置
      //  initialPosition = transform.position;
        initialRotation = transform.rotation;

        // 显示初始位置调试信息
        //Debug.Log($"Start: 初始位置: {initialPosition}");

        // 获取玩家动画控制器组件
        playerAnimator = this.gameObject.transform.GetComponent<Animator>();
       // Debug.Log($"Start: 获取Animator - {playerAnimator != null}");

        // 禁用动画控制器，防止在移动过程中播放动画
        if (playerAnimator != null)
        {
            playerAnimator.enabled = false;
          //  Debug.Log("Start: Animator组件已禁用");
        }

        // 获取玩家精灵渲染器组件
        playerSpriteRenderer = this.gameObject.transform.GetComponent<SpriteRenderer>();
        //Debug.Log($"Start: 获取SpriteRenderer - {playerSpriteRenderer != null}");

        // 禁用精灵渲染器，在延迟期间隐藏玩家
        if (playerSpriteRenderer != null)
        {
            playerSpriteRenderer.enabled = false;
           // Debug.Log("Start: Sprite Renderer已禁用");
        }

        // 获取玩家刚体2D组件
        playerRigidbody = this.gameObject.transform.GetComponent<Rigidbody2D>();

        // 禁用物理模拟，防止在移动过程中受物理影响
        if (playerRigidbody != null)
        {
            playerRigidbody.simulated = false;
           // Debug.Log("Start: Rigidbody2D物理模拟已禁用");
        }

        // 获取玩家控制脚本组件
        playerControl = this.gameObject.transform.GetComponent<PlayerControl>();

        // 禁用玩家控制，防止在移动过程中玩家输入干扰
        if (playerControl != null)
        {
            playerControl.enabled = false;
          //  Debug.Log("Start: PlayerControl脚本已禁用");
        }

       

        // 检查目标物体是否设置
        if (targetObject == null)
        {
            Debug.LogError("目标物体未设置！请在Inspector中指定目标物体。");
        }
        else
        {
            // 计算初始距离并显示调试信息
            float initialDistance = Vector2.Distance(transform.position, targetObject.transform.position);
           //
        }

        // 开始延迟激活协程
        StartCoroutine(DelayedActivation());
    }

    /// <summary>
    /// 延迟激活协程
    /// 等待指定时间后激活玩家和Gold，并开始移动
    /// </summary>
    IEnumerator DelayedActivation()
    {

        Debug.Log("等待2秒延迟时间...");
        // 等待指定的延迟时间
        yield return new WaitForSeconds(delayTime);

        Debug.Log("2秒延迟时间结束，开始激活玩家");
        

        // 激活玩家精灵渲染器，使玩家可见
        if (playerSpriteRenderer != null)
        {
            lightFlash.SetActive(true);
            playerSpriteRenderer.enabled = true;
            Debug.Log($"玩家Sprite Renderer已启用: {playerSpriteRenderer.enabled}");
        }


        // 设置激活状态和移动状态
        isActivated = true;
        isMoving = true;

        Debug.Log("开始移动到目标位置，Animator保持禁用");
        
        // 如果启用旋转效果，开始旋转协程
        if (enableSpinEffect)
        {
            StartCoroutine(SpinEffect());
        }
    }

    /// <summary>
    /// 每帧更新方法
    /// 处理玩家的移动逻辑和状态检查
    /// </summary>
    void Update()
    {
        // 如果未激活，直接返回
        if (!isActivated) return;

        // 检查目标物体是否存在
        if (targetObject == null)
        {
            if (showDebugInfo) Debug.LogWarning("目标物体为空，无法移动");
            return;
        }

        // 如果未移动或已到达，直接返回
        if (!isMoving || hasArrived) return;

        // 获取当前位置和目标位置
        Vector2 currentPos = transform.position;
        Vector2 targetPos = targetObject.transform.position;

        // 计算与目标的距离
        float distance = Vector2.Distance(currentPos, targetPos);

        // 定期显示移动信息（每30帧一次）
        if (showDebugInfo && Time.frameCount % 30 == 0)
            Debug.Log($"当前距离: {distance}, 移动速度: {moveSpeed}, 当前位置: {currentPos}");

        // 如果距离大于停止距离，继续移动
        if (distance > stoppingDistance)
        {
            // 使用MoveTowards方法向目标位置平滑移动
            transform.position = Vector2.MoveTowards(currentPos, targetPos, moveSpeed * Time.deltaTime);

            // 如果不启用旋转效果，计算并应用朝向目标的旋转
            if (!enableSpinEffect)
            {
                // 计算朝向目标的方向向量
                Vector2 direction = (targetPos - currentPos).normalized;

                // 计算方向的角度
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                // 创建目标旋转
                Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);

                // 应用旋转（平滑过渡或直接设置）
                transform.rotation = useSmoothRotation
                    ? Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime)
                    : targetRotation;
            }
        }
        else
        {
            // 到达目标位置，调用到达处理方法
            ArriveAtDestination();
        }
    }

    /// <summary>
    /// 旋转效果协程
    /// 在移动过程中持续旋转玩家，创造视觉特效
    /// </summary>
    private IEnumerator SpinEffect()
    {
        Debug.Log("开始旋转效果");

        // 计算总移动距离
        float journeyLength = Vector2.Distance(initialPosition, targetObject.transform.position);

        // 在移动过程中持续旋转
        while (isMoving && !hasArrived)
        {
            // 计算已移动的距离
            float distCovered = Vector2.Distance(initialPosition, transform.position);

            // 计算移动进度（0到1）
            float journeyFraction = distCovered / journeyLength;

            // 通过动画曲线获取旋转速度系数
            float curveValue = spinCurve.Evaluate(journeyFraction);

            // 计算当前帧的旋转角度
            float spinAngle = curveValue * spinSpeed * Time.deltaTime;

            // 应用旋转（绕Z轴旋转）
            transform.Rotate(0, 0, spinAngle);

            // 等待下一帧
            yield return null;
        }

        Debug.Log("旋转效果结束");
    }

    /// <summary>
    /// 到达目标位置的处理方法
    /// 启用所有组件，完成登场流程
    /// </summary>
    private void ArriveAtDestination()
    {
        // 更新移动状态
        isMoving = false;
        hasArrived = true;

        // 显示到达信息
        Debug.Log($"{gameObject.name} 已到达目标位置！");
        Debug.Log($"到达位置: {transform.position}, 目标位置: {targetObject.transform.position}");

        // 启用玩家动画控制器
        if (playerAnimator != null)
        {
            playerAnimator.enabled = true;
            Debug.Log("Animator组件已启用，开始播放Idle动画");
        }

        // 启用物理模拟
        if (playerRigidbody != null)
        {
            playerRigidbody.simulated = true;
            Debug.Log("Rigidbody2D物理模拟已启用");
        }

        // 启用玩家控制脚本
        if (playerControl != null)
        {
            playerControl.enabled = true;
            Debug.Log("PlayerControl脚本已启用");
        }

        // 重置旋转到初始状态
        transform.rotation = initialRotation;
        Debug.Log("旋转已重置到初始状态");

        // 开始位置监控协程，确保位置稳定
        StartCoroutine(MonitorPosition());
        dialogueTrigger.OnAnimationComplete();

    }

    /// <summary>
    /// 位置监控协程
    /// 在到达目标后的100帧内监控玩家位置，防止意外移动
    /// </summary>
    private IEnumerator MonitorPosition()
    {
        // 记录当前位置
        Vector3 lastPosition = transform.position;
        Debug.Log($"开始监控位置变化，初始位置: {lastPosition}");

        // 监控100帧
        for (int i = 0; i < 100; i++)
        {
            // 如果位置发生变化
            if (transform.position != lastPosition)
            {
                Debug.LogWarning($"位置发生变化! 帧 {i}: 从 {lastPosition} 变为 {transform.position}");

                // 强制设置回目标位置
                if (targetObject != null)
                {
                    transform.position = targetObject.transform.position;
                    Debug.Log($"强制设置位置到目标位置: {targetObject.transform.position}");
                }

                // 更新记录的位置
                lastPosition = transform.position;
            }

            // 等待下一帧
            yield return null;
        }

        Debug.Log("位置监控结束");
    }

    /// <summary>
    /// 延迟更新方法
    /// 在每帧的最后执行，用于纠正可能的问题
    /// </summary>
    private void LateUpdate()
    {
        // 如果已激活但精灵渲染器被意外禁用，重新启用
        if (isActivated && playerSpriteRenderer != null && !playerSpriteRenderer.enabled)
        {
            playerSpriteRenderer.enabled = true;
            Debug.LogWarning("LateUpdate: 检测到Sprite Renderer被禁用，已重新启用");
        }

        // 如果已到达但位置不正确，强制纠正
        if (hasArrived && targetObject != null && transform.position != targetObject.transform.position)
        {
            //transform.position = targetObject.transform.position;
            Debug.LogWarning("LateUpdate: 检测到位置不正确，已强制设置到目标位置");
        }
    }

    // ========== 公共方法 ==========

    /// <summary>
    /// 设置新目标
    /// 动态改变玩家的移动目标
    /// </summary>
    /// <param name="newTarget">新的目标物体</param>
    public void SetNewTarget(GameObject newTarget)
    {
        targetObject = newTarget;
        isMoving = true;
        hasArrived = false;
        Debug.Log($"新目标设置为：{newTarget.name}");
    }

    /// <summary>
    /// 开始移动
    /// 手动触发移动
    /// </summary>
    public void StartMoving()
    {
        isMoving = true;
        Debug.Log("开始移动");
    }

    /// <summary>
    /// 停止移动
    /// 手动停止移动
    /// </summary>
    public void StopMoving()
    {
        isMoving = false;
        Debug.Log("停止移动");
    }
}