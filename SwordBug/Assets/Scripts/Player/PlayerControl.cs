using DG.Tweening;
using DG.Tweening.Core;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Tilemaps;
using static UnityEditor.Progress;


public class PlayerControl : MonoBehaviour
{
    private PlayerInputControl inputActions;
    private Rigidbody2D rb;
    private SpriteRenderer sr;
    private PhysicsCheck pc;
    private PlayerAnimation pa;
    private CapsuleCollider2D coll;
    private Character character;
    [Header("监听事件")]
    public SceneLoadEventSO loadEvent;
    public VoidEventSO afterSceneLoadedEvent;
    public VoidEventSO loadDataEvent;
    public VoidEventSO onMap0Loaded; // map0加载事件
    public VoidEventSO backToMenuEvent;

   // 获取游戏逻辑实例
    private TetrisGameLogic gameLogic;
    [Header("音效")]
    public AudioDefination audioJump;
    public AudioDefination audioDoubleJump;
    public AudioDefination audioShrink;
    public AudioDefination audioRestore;
    public AudioDefination audioHurt;
    [Header("道具系统")]
    public PlayerInventory inventory;
   
    [Header("移动相关")]
    [SerializeField] private float runspeed = 200f;
    public Vector2 moveDirection;

    [Header("跳跃相关")]
    [SerializeField] private float jumpForce=10f;
    [SerializeField] private float jumpForce2 = 15f;
    public bool isDoubleJump = false;
    [SerializeField] private float doubleJumpCooldown = 0.3f; // 二段跳冷却时间

    private bool doubleJump; // 是否可以进行二段跳
    private bool canDoubleJump = true; // 二段跳是否可用
    private float lastJumpTime; // 上一次跳跃的时间

    [Header("受伤后坐力")]
    [SerializeField] private float hurtForce;

    [Header("状态查看")]
    public bool isHurt;
    public bool isDead;
    public bool isAttack;
    public bool isHold;//判断手里是否有东西
    public bool isPick;//是否捡取物品
    public bool canChangeSize = false; // 是否可以改变大小
    public bool isSmallSize = false; // 当前是否为小尺寸
    private bool canCollect;//能否收集方块
    public bool isInSizeChangeMode = false; // 是否处于可以改变大小的模式
    public bool isChangingSize = false; // 是否正在改变大小
    private bool isMovementReversed = false; // 移动方向是否反转
    public bool isTetromino=false;//是否在俄罗斯方块关卡
    public bool isMushroom;//是否进入第二关



    [Header("物理材质")]
    public PhysicsMaterial2D normal;
    public PhysicsMaterial2D wall;
    [Header("组件")]
    public GameObject enemy;
    public GameObject UI;
    public GameObject attackArea;
    public GameObject block;
    public Sign sign;
    private Vector2 attackDirec;
    public DialogueTrigger dialogueEnd;
    public PlayerAppearController appearController;
    [Header("撑杆跳")]
    [SerializeField] private float pvForce;//竖直力量
    [SerializeField] private float poleVaultHorizontalForce; // 水平方向力量
    [Header("大小改变")]
    [SerializeField] private float shrinkSpeed = 0.5f;
    public Vector3 originalScale;
    [SerializeField] private float minScale = 0.2f;
    // 用于跟踪按键状态
    private bool isScaleKeyPressed = false;
    private Coroutine sizeChangeCoroutine;
    private bool hasTriggered;

    private void Awake()
    {
       
        inputActions = new PlayerInputControl();
        rb=GetComponent<Rigidbody2D>();
        sr=GetComponent<SpriteRenderer>();
        pc=GetComponent<PhysicsCheck>();
        pa=GetComponent<PlayerAnimation>();
        coll=GetComponent<CapsuleCollider2D>();
        inventory = GetComponent<PlayerInventory>();
        character = GetComponent<Character>();

        attackDirec =attackArea.transform.localScale;
        originalScale=transform.localScale;

        RegisterInputAction();




    }

    public void RegisterInputAction()
    {
        inputActions.GamePlay.Enable();


        // 注册事件 
        inputActions.GamePlay.Jump.started += Jump;
        inputActions.GamePlay.Attack.started += PlayerAttack;
        inputActions.GamePlay.Use.started += PlayerUse;
        inputActions.GamePlay.Pick.started += PickUpItem;
    }
    private void Start()
    {
        rb.simulated = false;
    }

    /// <summary>
    /// 切换输入方式（此时碰完蘑菇）
    /// </summary>
    public void ToggleInputSystem()
    {
        // 切换移动方向反转状态
        isMovementReversed = !isMovementReversed;
        Debug.LogWarning("切换方向");

        // 切换大小改变模式
        isInSizeChangeMode = !isInSizeChangeMode;

        if (isInSizeChangeMode)
        {
            // 启用大小改变功能
            inputActions.GamePlay.Change.started += OnScaleKeyPressed;
            inputActions.GamePlay.Change.canceled += OnScaleKeyReleased;
        }
        else
        {
            // 禁用大小改变功能
            inputActions.GamePlay.Change.started -= OnScaleKeyPressed;
            inputActions.GamePlay.Change.canceled -= OnScaleKeyReleased;

            // 当退出大小改变模式时，自动恢复到原始大小
            RestoreToOriginalSize();
            isMovementReversed=false;
       
        }

        Debug.Log($"移动方向{(isMovementReversed ? "已反转" : "恢复正常")}");
        Debug.Log($"大小改变模式: {(isInSizeChangeMode ? "启用" : "禁用")}");
    }

    /// <summary>
    /// 按下缩放键时的处理
    /// </summary>
    private void OnScaleKeyPressed(InputAction.CallbackContext context)
    {
        if (!isInSizeChangeMode) return;

        // 在大小改变模式下，只能缩小
        audioShrink.PlayAudioClip();
        StartGradualShrink();
    }

    /// <summary>
    /// 松开缩放键时的处理
    /// </summary>
    private void OnScaleKeyReleased(InputAction.CallbackContext context)
    {
        if (!isInSizeChangeMode) return;

        // 停止渐进缩小
        StopSizeChange();
    }

    /// <summary>
    /// 开始渐进缩小
    /// </summary>
    private void StartGradualShrink()
    {
        Debug.Log($"开始渐进缩小 - 模式: {isInSizeChangeMode}, 正在改变: {isChangingSize}");
        if (!isInSizeChangeMode || isChangingSize)
        {
            Debug.LogWarning($"无法开始缩小: 模式={isInSizeChangeMode}, 正在改变={isChangingSize}");
            return;
        }


        isScaleKeyPressed = true;
        isChangingSize = true;

        if (sizeChangeCoroutine != null)
        {
            Debug.Log("停止之前的协程");
            StopCoroutine(sizeChangeCoroutine);
        }
           

        sizeChangeCoroutine = StartCoroutine(GradualShrinkProcess());
        Debug.Log("启动渐进缩小协程");
    }

    /// <summary>
    /// 停止大小改变
    /// </summary>
    private void StopSizeChange()
    {
        isScaleKeyPressed = false;
        isChangingSize = false;

        if (sizeChangeCoroutine != null)
        {
            StopCoroutine(sizeChangeCoroutine);
            sizeChangeCoroutine = null;
        }
    }

    /// <summary>
    /// 渐进缩小协程
    /// </summary>
    private IEnumerator GradualShrinkProcess()
    {
        while (isScaleKeyPressed && isInSizeChangeMode)
        {
            // 计算新的缩放值
            Vector3 newScale = transform.localScale - Vector3.one * shrinkSpeed * Time.deltaTime;
            Vector3 newShrinkScale = newScale;
            // 限制缩放不能小于最小尺寸
            newShrinkScale.x = Mathf.Max(newScale.x, minScale);
            newShrinkScale.y = Mathf.Max(newScale.y, minScale);
            newShrinkScale.z = Mathf.Max(newScale.z, minScale);
            // 应用新的缩放

            transform.localScale = newShrinkScale;
            // 添加详细的调试信息
            Debug.Log($"缩放更新: {transform.localScale}, isScaleKeyPressed: {isScaleKeyPressed}, isInSizeChangeMode: {isInSizeChangeMode}");
            transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
            // 如果达到最小尺寸且不是死亡状态，造成伤害
            if (newScale.x <= minScale && !isDead)
            {
                character.TakeDamge();
                StopSizeChange();
                break;
            }

            yield return null;
        }
        Debug.Log("退出渐进缩小协程，原因: " +
               (!isScaleKeyPressed ? "按键已释放" : "退出大小改变模式") +
               ", isInSizeChangeMode: " + isInSizeChangeMode);
        isChangingSize = false;
        sizeChangeCoroutine = null;
    }

    /// <summary>
    /// 恢复到原始大小
    /// </summary>
    private void RestoreToOriginalSize()
    {
        if (sizeChangeCoroutine != null)
            StopCoroutine(sizeChangeCoroutine);
        audioRestore.PlayAudioClip();
        sizeChangeCoroutine = StartCoroutine(RestoreSizeProcess());
    }

    /// <summary>
    /// 恢复大小协程（平滑过渡）
    /// </summary>
    private IEnumerator RestoreSizeProcess()
    {
        isChangingSize = true;
        isMovementReversed = false;

        Vector3 startSize = transform.localScale;
        float duration = 0.3f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            transform.localScale = Vector3.Lerp(startSize, originalScale, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = originalScale;
        isChangingSize = false;
        sizeChangeCoroutine = null;

        Debug.Log("已恢复到原始大小");
    }
   
   

  

    void Update()
    {
        if (isChangingSize && sizeChangeCoroutine == null)
        {
            Debug.LogWarning("异常：isChangingSize为true但协程为null");
        }

        if (!isAttack) ReadMovement();
        UpdateDoubleJumpStatus();
        CheckState();
        if (character.isRestart)
        {
            if (!isMovementReversed&&!isTetromino&&isMushroom)
            {
                ToggleInputSystem();//设置为刚吃下第一个蘑菇的状态
                character.isRestart = false;
            }
            Item sword = transform.GetComponentInChildren<Item>();
            if (sword == null) character.isRestart = false;
            if (sword.tag == "Sword")
            {
                Destroy(sword.gameObject);
                character.isRestart = false;
            }
            if (block.activeInHierarchy)
            {
                block.SetActive(false);
                character.isRestart = false;

            }
        }

        
    }

   

    private void FixedUpdate()
    {
        if(!isHurt&&!isAttack) Move();
    }
    private void OnEnable()
    {
        inputActions.Enable();
        loadEvent.LoadRequestEvent += OnLoadEvent;
        afterSceneLoadedEvent.OnEventRaised += OnAfterSceneLoadedEvent;
        loadDataEvent.OnEventRaised += OnLoadDataEvent;
        onMap0Loaded.OnEventRaised += OnMap0Loaded;
        backToMenuEvent.OnEventRaised += OnLoadDataEvent;

    }

    private void OnMap0Loaded()
    {
        rb.simulated=true;
        appearController.enabled = true;

    }

    /// <summary>
    /// 读取游戏进度
    /// </summary>
    private void OnLoadDataEvent()
    {
       isDead = false;
       
    }

    private void OnAfterSceneLoadedEvent()
    {
       inputActions.GamePlay.Enable();
    }

    private void OnLoadEvent(GameSceneSO arg0, Vector3 arg1, bool arg2, bool i)
    {
       inputActions.GamePlay.Disable();

    }

    private void OnDisable()
    {
        inputActions.Disable();
        loadEvent.LoadRequestEvent -= OnLoadEvent;
        afterSceneLoadedEvent.OnEventRaised -= OnAfterSceneLoadedEvent;
        loadDataEvent.OnEventRaised -= OnLoadDataEvent;
        onMap0Loaded.OnEventRaised -= OnMap0Loaded;
        backToMenuEvent.OnEventRaised -= OnLoadDataEvent;

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
        Vector2 inputDirection = inputActions.GamePlay.Move.ReadValue<Vector2>();

        if (isMovementReversed)
        {
            // 只反转X轴，保持Y轴不变
            moveDirection = new Vector2(-inputDirection.x, inputDirection.y);
           // Debug.Log($"反向移动 - 输入: {inputDirection}, 实际: {moveDirection}");
        }
        else
        {
            moveDirection = inputDirection;
          //  Debug.Log($"正常移动 - 方向: {moveDirection}");
        }

    }

    private void Jump(InputAction.CallbackContext context)
    {
        // 在地面上跳跃
        if (pc.isGround)
        {
            audioJump.PlayAudioClip();
            rb.velocity = new Vector2(rb.velocity.x, 0); // 重置Y轴速度，确保跳跃高度一致
            rb.AddForce(transform.up * jumpForce, ForceMode2D.Impulse);
            doubleJump = true; // 在地面上进行一阶段跳跃，获得二段跳条件
            lastJumpTime = Time.time;
            canDoubleJump = true; // 重置二段跳可用状态
        }
        // 在空中且可以进行二段跳且二段跳可用
        else if (!pc.isGround && doubleJump && canDoubleJump && !isDoubleJump)
        {
            audioDoubleJump.PlayAudioClip();
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
        // 只有捡取了物品时才能攻击
        if (!isPick)
        {
            Debug.Log("无法攻击：手中没有物品");
            return;
        }

        // 如果已经在攻击中，不允许再次攻击
        if (isAttack) return;

        pa.PlayAttack();
        isAttack = true;
    }  
    public void ActiveUI()
    {
        UI.SetActive(true);
    }
    public void ActiveArea(bool i)
    {
       attackArea.SetActive(i);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
       
        gameLogic = FindObjectOfType<TetrisGameLogic>();
        Tilemap tilemap = collision.gameObject.GetComponent<Tilemap>();
        if (tilemap != null)
        {
            // 获取所有碰撞点
            foreach (ContactPoint2D contact in collision.contacts)
            {
                Vector3 hitPosition = contact.point;
                Vector3Int cellPosition = tilemap.WorldToCell(hitPosition);
                TileBase hitTile = tilemap.GetTile(cellPosition);

                if (gameLogic != null)
                {
                    enemy.SetActive(false);
                    isTetromino=true;
                    // 检查是否是当前活动的俄罗斯方块
                    if (gameLogic.IsActiveTetrisTile(cellPosition, hitTile))
                    {
                        if (gameLogic.CurrentTetris != null && gameLogic.CurrentTetris.isCollectable)
                        {
                            //nearbyCollectableTetris = gameLogic.CurrentTetris;
                            canCollect = true;
                            sign.ChangeSign(true);
                            Debug.Log("检测到可收集方块，按E键收集");
                            
                        }
                        gameLogic.HandlePlayerTetrisCollision(cellPosition, contact.normal, this);
                        break; // 处理一次碰撞即可
                    }
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        sign.ChangeSign(false);
    }

    // 添加持续碰撞检测，但只检测活动俄罗斯方块
    private void OnCollisionStay2D(Collision2D collision)
    {
        gameLogic = FindObjectOfType<TetrisGameLogic>();

        Tilemap tilemap = collision.gameObject.GetComponent<Tilemap>();
        if (tilemap != null && Time.frameCount % 15 == 0) // 降低检测频率
        {
            ContactPoint2D contact = collision.contacts[0];
            Vector3 hitPosition = contact.point;
            Vector3Int cellPosition = tilemap.WorldToCell(hitPosition);
            TileBase hitTile = tilemap.GetTile(cellPosition);

            TetrisGameLogic gameLogic = FindObjectOfType<TetrisGameLogic>();
            if (gameLogic != null && gameLogic.IsActiveTetrisTile(cellPosition, hitTile))
            {
                gameLogic.HandlePlayerTetrisCollision(cellPosition, contact.normal, this);
            }
        }
    }
    private void PickUpItem(InputAction.CallbackContext context)
    {
       // 检测附近的道具
        Collider2D[] nearbyItems = Physics2D.OverlapCircleAll(transform.position, 1.5f);

        foreach (var collider in nearbyItems)
        {
            Item item = collider.GetComponent<Item>();
            if (item != null)
            {
                inventory.PickupItem(item);//将item设为子物体跟随
                                           // 播放对应动画
                if (item.itemType == Tools.Sword) enemy.SetActive(true);
                if(item.itemType == Tools.Phone) dialogueEnd.ChangeisDialogue();
                isPick = true;
                isHold = true;
                return;
            }
        }
        if (canCollect)
        {
            gameLogic.CurrentTetris.isCollected = true;
            block.SetActive(true);
            inventory.PickupItem(block.GetComponent<Item>());
            isPick = true;
            isHold = true;
            sign.ChangeSign(false);
            return;
        }
        Debug.Log("附近没有可拾取的道具");
    }
    private void PlayerUse(InputAction.CallbackContext context)
    {
        if (!inventory.HasItem())
        {
            Debug.Log("无法使用：手中没有物品");
            return;
        }

        // 根据道具类型执行不同操作
        if (inventory.HasItem(Tools.Sword))
        {
            PerformPoleVault();
        }
        // 使用道具
        inventory.UseCurrentItem();
        isHold = false;

    }

    //撑杆跳
    private void PerformPoleVault()
    {

        // 设置撑杆跳状态
       // isPoleVaulting = true;
        // 播放撑杆跳动画
        pa.UseSword();

        // 应用撑杆跳物理效果
        ApplyPoleVaultPhysics();

        

    }

    private void ApplyPoleVaultPhysics()
    {
        if (pc.isCelling || pc.touchLeftWall || pc.touchRightWall) return;
        // 重置当前速度
        rb.velocity = Vector2.zero;
        // 根据朝向决定水平方向
       
       Vector2 poleVaultDirection = new Vector2(sr.flipX ? -1f : 1f, 0);
        // 应用初始跳跃力量
        rb.AddForce(Vector2.up * pvForce, ForceMode2D.Impulse);

        // 应用水平力量
        rb.AddForce(poleVaultDirection * poleVaultHorizontalForce, ForceMode2D.Impulse);

        Debug.Log("chenggantiao");

    }

    public void GetHurt(Transform attack)
    {
        isHurt = true;
        audioHurt.PlayAudioClip();
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
        if (moveDirection.x < 0)//左
        {
            sr.flipX = true;
            attackArea.transform.localScale=new Vector2(attackDirec.x, attackDirec.y);
        }
        else if (moveDirection.x > 0)//右
        {
            sr.flipX = false;
            attackArea.transform.localScale = new Vector2(-attackDirec.x, attackDirec.y);
        }
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
            inputActions.GamePlay.Pick.started -= PickUpItem;
            inputActions.GamePlay.Change.started -= OnScaleKeyPressed;
            inputActions.GamePlay.Change.canceled -= OnScaleKeyReleased;

        }
    }
}
