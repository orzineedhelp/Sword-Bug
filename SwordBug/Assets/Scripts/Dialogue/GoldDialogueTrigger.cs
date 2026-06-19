using DG.Tweening; // 引入DOTween动画库
using System.Collections;
using System.Collections.Generic;
using TMPro; // TextMeshPro命名空间
using UnityEngine;
using UnityEngine.InputSystem; // 新输入系统
using UnityEngine.UI;

public class GoldDialogueTrigger : MonoBehaviour
{
    [Header("触发方式")]
    public TriggerMode triggerMode = TriggerMode.AutoAfterAnimation; // 触发方式选择：动画后自动/位置触发/手动
    public float triggerDistance = 1.0f; // 触发对话的距离阈值

    [Header("引用")]
    public GameObject dialogueBox; // Gold的对话框UI
    public GameObject playerDialogueBox; // 玩家的对话框UI
    public TextMeshProUGUI goldText; // Gold的文本显示组件
    public TextMeshProUGUI playerText; // 玩家的文本显示组件

    [Header("角色头像")]
    public Image goldImage;    // Gold的头像显示

    [Header("对话内容")]
    public List<DialogueEntry> dialogueEntries = new List<DialogueEntry>(); // 对话条目列表

    [Header("打字机效果设置")]
    public float typingSpeed = 0.05f; // 每个字符的打字速度
    public float startDelay = 0.2f; // 开始打字前的延迟

    [Header("情绪图标设置")]
    public float emotionIconScaleDuration = 0.8f; // 情绪图标缩放动画时长

    [Header("自动触发设置")]
    public float autoStartDelay = 1.0f; // 动画结束后延迟开始对话的时间

    // 私有变量
    public bool dialogueBrokein = false;//对话是否被打断
    public bool dialogueActive = false; // 对话是否正在进行中
    public bool dialogueCompleted = false; // 对话是否已完成
    private int currentDialogueIndex = 0; // 当前对话条目的索引
    private Tween typewriterTween; // 存储打字机效果的Tween对象
    private bool canProceedToNext = false; // 是否可以进入下一句对话
    private GameObject currentEmotionIcon; // 当前显示的情绪图标对象
    private PlayerInputControl inputActions; // 输入控制系统
    private bool waitingForAnimation = true; // 是否在等待动画完成
    public GameObject player;
    private bool isPlayerInRange = false; // 玩家是否在触发范围内
    private bool canInteract = true; // 是否可以交互

    // 触发模式枚举定义
    public enum TriggerMode
    {
        AutoAfterAnimation, // 动画后自动触发
        Manual,              // 手动触发
        Proximity           // 位置触发
    }

    // 对话条目数据结构
    [System.Serializable]
    public class DialogueEntry
    {
        [TextArea(2, 4)] // 在Inspector中显示多行文本区域
        public string text; // 对话文本内容
        public bool isGoldSpeaking = true; // 说话者标识：true=Gold说话, false=玩家说话
        public GameObject emotionIcon; // 关联的情绪图标GameObject
    }

    private void Awake()
    {
        inputActions = new PlayerInputControl(); // 初始化输入控制系统
    }

    private void OnEnable()
    {
        inputActions.Enable(); // 启用输入系统
    }

    private void OnDisable()
    {
        inputActions.Disable(); // 禁用输入系统
    }

    void Start()
    {
        // 初始隐藏所有对话框UI元素
        if (dialogueBox != null) dialogueBox.SetActive(false);
        if (playerDialogueBox != null) playerDialogueBox.SetActive(false);
        if (goldImage != null) goldImage.gameObject.SetActive(false);

        // 隐藏所有文本组件
        if (goldText != null) goldText.gameObject.SetActive(false);
        if (playerText != null) playerText.gameObject.SetActive(false);

        // 确保所有情绪图标初始状态为隐藏
        EnsureEmotionIconsAreHidden();

        // 根据选择的触发模式进行初始化
        InitializeByTriggerMode();
    }

    // 根据触发模式进行不同的初始化设置
    void InitializeByTriggerMode()
    {
        switch (triggerMode)
        {
            case TriggerMode.AutoAfterAnimation:
                waitingForAnimation = true; // 等待动画完成标志
                //  Debug.Log("等待动画完成后自动开始对话");
                break;
            case TriggerMode.Manual:
                waitingForAnimation = false; // 不需要等待动画
                Debug.Log("等待手动触发对话");
                break;
            case TriggerMode.Proximity:
                waitingForAnimation = false; // 不需要等待动画
                Debug.Log("位置触发模式已启用");
                break;
        }
    }

    private void PauseGame(bool isStop)
    {
        if (isStop)
        {
            player.GetComponent<PlayerControl>().enabled = false;
            player.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            // 暂停输入
        }
        else
        {
            player.GetComponent<PlayerControl>().enabled = true;
        }
    }

    // 确保所有对话条目中的情绪图标在场景启动时都是隐藏状态
    void EnsureEmotionIconsAreHidden()
    {
        foreach (DialogueEntry entry in dialogueEntries)
        {
            if (entry.emotionIcon != null)
            {
                entry.emotionIcon.SetActive(false); // 隐藏情绪图标
            }
        }
    }

    void Update()
    {
        // 检测对话输入：当对话激活时，检测鼠标点击输入
        if (dialogueActive && inputActions.UI.Click.WasPressedThisFrame())
        {
            HandleDialogueInput(); // 处理对话输入
        }
    }

    // 触发器进入检测
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (triggerMode == TriggerMode.Proximity &&
            !dialogueActive &&
            !dialogueCompleted &&
            canInteract &&
            collision.CompareTag("Player"))
        {
            StartDialogue();
        }
    }

    // 公共方法：在动画完成后调用（用于AutoAfterAnimation触发模式）
    public void OnAnimationComplete()
    {
        if (triggerMode == TriggerMode.AutoAfterAnimation && waitingForAnimation)
        {
            Debug.Log("动画完成，准备开始对话");
            waitingForAnimation = false; // 标记动画已完成
            StartCoroutine(StartDialogueAfterDelay()); // 延迟后开始对话
        }
    }

    // 协程：在指定延迟后开始对话
    IEnumerator StartDialogueAfterDelay()
    {
        yield return new WaitForSeconds(autoStartDelay); // 等待指定延迟时间
        PauseGame(true);
        StartDialogue(); // 开始对话
    }

    // 公共方法：开始对话序列
    public void StartDialogue()
    {
        // 检查条件：对话条目为空、对话已激活或已完成时直接返回
        if (dialogueEntries.Count == 0 || dialogueActive || dialogueCompleted)
            return;

        dialogueActive = true; // 标记对话为激活状态
        currentDialogueIndex = 0; // 重置对话索引

        // 显示对话框
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(true);
        }

        // 显示第一个对话条目
        ShowCurrentDialogue();

        Debug.Log("对话开始");
    }

    /// <summary>
    /// 展示当前对话条目
    /// </summary>
    void ShowCurrentDialogue()
    {
        // 隐藏上一句话的情绪图标
        if (currentEmotionIcon != null)
        {
            currentEmotionIcon.SetActive(false);
            currentEmotionIcon = null;
        }

        // 检查是否已到达对话末尾
        if (currentDialogueIndex >= dialogueEntries.Count)
        {
            EndDialogue(); // 结束对话
            return;
        }

        // 获取当前对话条目
        DialogueEntry currentEntry = dialogueEntries[currentDialogueIndex];

        // 根据说话者身份设置不同的UI显示
        if (currentEntry.isGoldSpeaking)
        {
            // Gold说话时的UI设置
            if (dialogueBox != null) dialogueBox.SetActive(true);
            if (playerDialogueBox != null) playerDialogueBox.SetActive(false);
            if (goldImage != null) goldImage.gameObject.SetActive(true);

            // 设置文本显示：显示Gold文本，隐藏玩家文本
            if (goldText != null)
            {
                goldText.gameObject.SetActive(true);
                playerText.gameObject.SetActive(false);
                StartTypewriterEffect(currentEntry.text, goldText); // 启动打字机效果
            }
        }
        else
        {
            // 玩家说话时的UI设置
            if (dialogueBox != null) dialogueBox.SetActive(false);
            if (playerDialogueBox != null) playerDialogueBox.SetActive(true);
            if (goldImage != null) goldImage.gameObject.SetActive(false);

            // 设置文本显示：显示玩家文本，隐藏Gold文本
            if (playerText != null)
            {
                playerText.gameObject.SetActive(true);
                goldText.gameObject.SetActive(false);
                StartTypewriterEffect(currentEntry.text, playerText); // 启动打字机效果
            }
        }

        canProceedToNext = false; // 重置下一句对话标志
    }

    // 启动打字机效果：逐字显示文本
    void StartTypewriterEffect(string text, TextMeshProUGUI targetText)
    {
        // 清除之前可能存在的打字效果
        if (typewriterTween != null && typewriterTween.IsActive())
        {
            typewriterTween.Kill();
        }

        targetText.text = ""; // 清空文本
        float duration = text.Length * typingSpeed; // 计算总打字时长

        int charCount = 0; // 当前已显示字符数

        // 使用DOTween创建打字机动画
        typewriterTween = DOTween.To(
            () => charCount, // 获取当前值
            x => {
                charCount = x;
                targetText.text = text.Substring(0, charCount); // 更新文本显示
            },
            text.Length, // 目标值：文本总长度
            duration // 持续时间
        )
        .SetDelay(startDelay) // 设置开始延迟
        .SetEase(Ease.Linear) // 设置缓动类型为线性
        .OnComplete(() =>
        {
            canProceedToNext = true; // 标记可以进入下一句
            ShowEmotionIcon(); // 显示情绪图标
        });
    }

    // 显示当前对话条目的情绪图标
    void ShowEmotionIcon()
    {
        DialogueEntry currentEntry = dialogueEntries[currentDialogueIndex];

        // 检查当前对话条目是否有情绪图标
        if (currentEntry.emotionIcon != null)
        {
            currentEmotionIcon = currentEntry.emotionIcon; // 设置当前情绪图标
            currentEmotionIcon.SetActive(true); // 激活图标

            // 保存原始缩放值
            Vector3 originalScale = currentEmotionIcon.transform.localScale;

            // 设置初始大小为0（从无到有的动画效果）
            currentEmotionIcon.transform.localScale = Vector3.zero;

            // 使用DOTween创建缩放动画
            currentEmotionIcon.transform.DOScale(originalScale, emotionIconScaleDuration)
                .SetEase(Ease.OutBack); // 使用回弹效果

            // Debug.Log($"显示情绪图标: {currentEmotionIcon.name}");
        }
    }

    // 处理对话过程中的输入
    void HandleDialogueInput()
    {
        if (!canProceedToNext)
        {
            // 如果打字效果还没完成，立即完成当前打字
            CompleteTypewriter();
            return;
        }

        // 继续到下一句对话
        currentDialogueIndex++;
        ShowCurrentDialogue();
    }

    /// <summary>
    /// 立即完成当前打字机效果
    /// </summary>
    void CompleteTypewriter()
    {
        if (typewriterTween != null && typewriterTween.IsActive())
        {
            typewriterTween.Complete(); // 立即完成Tween动画
        }
        else
        {
            // 手动完成文本显示（Tween不存在的情况）
            DialogueEntry currentEntry = dialogueEntries[currentDialogueIndex];
            if (currentEntry.isGoldSpeaking)
            {
                goldText.text = currentEntry.text; // 直接显示完整文本
            }
            else
            {
                playerText.text = currentEntry.text; // 直接显示完整文本
            }

            ShowEmotionIcon(); // 显示情绪图标
        }
        canProceedToNext = true; // 标记可以进入下一句
    }

    // 结束对话序列
    void EndDialogue()
    {
        dialogueActive = false; // 标记对话结束
        dialogueCompleted = true; // 标记对话已完成

        // 隐藏当前情绪图标
        if (currentEmotionIcon != null)
        {
            currentEmotionIcon.SetActive(false);
            currentEmotionIcon = null;
        }

        // 隐藏所有对话框UI
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
            playerDialogueBox.SetActive(false);
        }
        if (goldImage != null) goldImage.gameObject.SetActive(false);

        // 重置文本内容并隐藏
        if (goldText != null)
        {
            goldText.text = "";
            goldText.gameObject.SetActive(false);
        }
        if (playerText != null)
        {
            playerText.text = "";
            playerText.gameObject.SetActive(false);
        }

        Debug.Log("对话结束");
        PauseGame(false);
    }

    // 公共方法：手动触发对话（供外部调用）
    public void TriggerDialogueManually()
    {
        if (!dialogueActive && !dialogueCompleted)
        {
            StartDialogue();
        }
    }

    // 公共方法：重置对话状态（可重新开始对话）
    public void ResetDialogue()
    {
        dialogueActive = false;
        dialogueCompleted = false;
        currentDialogueIndex = 0;
        waitingForAnimation = (triggerMode == TriggerMode.AutoAfterAnimation); // 根据模式重置等待状态

        // 隐藏当前情绪图标
        if (currentEmotionIcon != null)
        {
            currentEmotionIcon.SetActive(false);
            currentEmotionIcon = null;
        }

        // 位置触发模式：重置交互状态
        if (triggerMode == TriggerMode.Proximity)
        {
            canInteract = true;
        }

        // 停止并清除打字机Tween
        if (typewriterTween != null && typewriterTween.IsActive())
        {
            typewriterTween.Kill();
        }
    }

    public void ForceCloseDialogue()
    {
        ResetDialogue();

        // 隐藏对话框和头像
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }
        if (playerDialogueBox != null)
        {
            playerDialogueBox.SetActive(false);
        }
        if (goldImage != null) goldImage.gameObject.SetActive(false);

        // 隐藏所有文本
        if (goldText != null) goldText.gameObject.SetActive(false);
        if (playerText != null) playerText.gameObject.SetActive(false);

        // 确保所有情绪图标是隐藏的
        EnsureEmotionIconsAreHidden();

        // 恢复玩家控制
        PauseGame(false);

        Debug.Log($"对话已强制关闭: {gameObject.name}");
    }

    // 位置触发模式：禁用交互（例如在特定情况下不允许触发对话）
    public void SetInteractable(bool interactable)
    {
        canInteract = interactable;
    }

    // 在编辑器中可视化触发范围
    private void OnDrawGizmosSelected()
    {
        if (triggerMode == TriggerMode.Proximity)
        {
            // 绘制触发范围
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, triggerDistance);
        }
    }
}