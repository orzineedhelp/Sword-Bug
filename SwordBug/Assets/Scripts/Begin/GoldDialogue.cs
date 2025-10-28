using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GoldDialogue : MonoBehaviour
{
    [Header("UI引用")]
    public GameObject dialogueBox;
    public GameObject PlayerDialogueBox;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI playerText;

    [Header("角色头像")]
    public GameObject goldImage;

    [Header("对话内容")]
    public List<DialogueEntry> dialogueEntries = new List<DialogueEntry>();

    [Header("打字机效果设置")]
    public float typingSpeed = 0.05f;
    public float startDelay = 0.2f;

    [Header("激活物体设置")]
    public GameObject objectToActivate;

    [Header("玩家设置")]
    public string playerTag = "Player";
    public float moveSpeed = 3f;
    public float stoppingDistance = 1f;

    [Header("Gold下降设置")]
    public float descentDistance = 3.86f; // 下降距离
    public float descentDuration = 2f;    // 下降持续时间

    [Header("特效设置")]
    public GameObject effectToActivate; // Gold下降后要激活的特效

    [Header("消失动画设置")]
    public float disappearDelay = 2f;    // 玩家到达后触发消失动画的延迟时间

    // 状态变量
    private bool isDialogueActive = false;
    private Tween typewriterTween;
    private int currentDialogueIndex = 0;
    private bool isDialogueComplete = false;
    private bool firstDialogueActivated = false;
    private bool hasDialogueBeenShown = false;
    private bool isMovingToGold = false;
    private GameObject player;
    private Animator playerAnimator;
    private Animator goldAnimator; // Gold的动画控制器
    private Tween moveTween;
    private Vector3 originalPosition; // Gold的原始位置
    private bool hasDisappeared = false; // 标记是否已经播放过消失动画
    private bool playerArrived = false; // 标记玩家是否已到达Gold身边
    private PlayerInputControl inputActions;

    private void Awake()
    {
        inputActions=new PlayerInputControl();
    }
    private void OnEnable()
    {
        inputActions.Enable();
    }
    [System.Serializable]
    public class DialogueEntry
    {
        public bool isGold;
        public string text;
    }

    void Start()
    {
        // 保存Gold的原始位置
        originalPosition = transform.position;

        // 获取玩家对象
        player = GameObject.FindGameObjectWithTag(playerTag);
        if (player != null)
        {
            playerAnimator = player.GetComponent<Animator>();
        }

        // 获取Gold的Animator组件
        goldAnimator = GetComponent<Animator>();

        // 确保对话框初始状态为未激活
        if (dialogueBox != null)
            dialogueBox.SetActive(false);
        if(PlayerDialogueBox != null)
            PlayerDialogueBox.SetActive(false);

        // 确保头像初始状态为未激活
        if (goldImage != null)
            goldImage.SetActive(false);
       

        // 确保要激活的物体初始状态为未激活
        if (objectToActivate != null)
            objectToActivate.SetActive(false);

        // 确保特效初始状态为未激活
        if (effectToActivate != null)
            effectToActivate.SetActive(false);
    }

    void Update()
    {
        // 检查鼠标左键点击且物体2已激活且没有对话在进行且对话从未显示过
        if (inputActions.UI.Click.triggered && gameObject.activeInHierarchy && !isDialogueActive && !hasDialogueBeenShown)
        {
            StartDialogue();
        }

        // 如果对话正在进行中，处理鼠标左键点击继续
        if (isDialogueActive && (inputActions.UI.Click.triggered))
        {
            HandleMouseClick();
        }

        // 检查玩家是否已到达Gold身边且尚未播放消失动画
        if (playerArrived && !hasDisappeared && !isMovingToGold)
        {
            // 玩家已到达且停止移动，触发消失动画
            StartCoroutine(TriggerDisappearAnimation());
            //TODO转场
            Addressables.LoadSceneAsync("Persistent");
        }
    }

    // 触发消失动画
    private IEnumerator TriggerDisappearAnimation()
    {
        hasDisappeared = true;

        // 等待指定的延迟时间
        yield return new WaitForSeconds(disappearDelay);

        // 播放玩家消失动画
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("IsDisappear", true);
        }
        objectToActivate.SetActive(false);
        // 播放Gold消失动画
        if (goldAnimator != null)
        {
            goldAnimator.SetBool("IsDisappear", true);
        }
    }

    public void StartDialogue()
    {
        if (dialogueBox != null && goldText != null && playerText != null && dialogueEntries.Count > 0)
        {
            isDialogueActive = true;
            isDialogueComplete = false;
            dialogueBox.SetActive(true);

            // 如果是第一次激活对话框，激活指定物体
            if (!firstDialogueActivated && objectToActivate != null)
            {
                objectToActivate.SetActive(true);
                firstDialogueActivated = true;
            }

            ShowCurrentDialogue();
        }
    }

    private void ShowCurrentDialogue()
    {
        if (currentDialogueIndex >= dialogueEntries.Count)
        {
            EndDialogue();
            return;
        }

        DialogueEntry currentEntry = dialogueEntries[currentDialogueIndex];

        // 根据说话者设置UI
        if (currentEntry.isGold)
        {
            // 神说话 - 激活神的对话框，关闭玩家对话框
            if (dialogueBox != null)
            {
                dialogueBox.SetActive(true);
            }
            if (PlayerDialogueBox != null)
            {
                PlayerDialogueBox.SetActive(false);
            }

            goldText.gameObject.SetActive(true);
            playerText.gameObject.SetActive(false);
            if (goldImage != null) goldImage.SetActive(true);
            StartTypewriterEffect(currentEntry.text, goldText);
        }
        else
        {
            // 玩家说话 - 激活玩家对话框，关闭神的对话框
            if (dialogueBox != null)
            {
                dialogueBox.SetActive(false);
            }
            if (PlayerDialogueBox != null)
            {
                PlayerDialogueBox.SetActive(true);
            }

            playerText.gameObject.SetActive(true);
            goldText.gameObject.SetActive(false);
            if (goldImage != null) goldImage.SetActive(false);
            StartTypewriterEffect(currentEntry.text, playerText);
        }
    }

    private void StartTypewriterEffect(string text, TextMeshProUGUI targetText)
    {
        if (typewriterTween != null && typewriterTween.IsActive())
            typewriterTween.Kill();

        targetText.text = "";
        float duration = text.Length * typingSpeed;

        int charCount = 0;

        typewriterTween = DOTween.To(
            () => charCount, // 获取当前值
            x =>
            {
                charCount = x;
                targetText.text = text.Substring(0, charCount); // 更新文本内容
            },
            text.Length, // 目标值（文本长度）
            duration // 持续时间
        )
        .SetDelay(startDelay)
        .SetEase(Ease.Linear);

    }

    private void HandleMouseClick()
    {
        if (typewriterTween != null && typewriterTween.IsActive())
        {
            CompleteTypewriter();
        }
        else
        {
            currentDialogueIndex++;

            if (currentDialogueIndex >= dialogueEntries.Count)
            {
                StartPlayerMovement();
            }
            else
            {
                ShowCurrentDialogue();
            }
        }
    }

    // 开始玩家移动
    private void StartPlayerMovement()
    {
        if (player != null)
        {
            // 播放跑步动画
            if (playerAnimator != null)
            {
                playerAnimator.SetBool("IsRun", true);
            }

            // 开始移动
            MovePlayerToGold();

            // 开始Gold下降
            StartGoldDescent();
        }
        else
        {
            EndDialogue();
        }
    }

    // 开始Gold下降
    private void StartGoldDescent()
    {
       
    // 计算目标位置（Y轴减少指定距离）
    Vector3 targetPosition = originalPosition - new Vector3(0, descentDistance, 0);

    // 使用DOTween实现缓慢下降
    transform.DOMove(targetPosition, descentDuration)
        .SetEase(Ease.OutCubic)
        .OnComplete(() =>
        {
            // 下降完成后激活特效
            if (effectToActivate != null)
            {
                effectToActivate.SetActive(true);
            }
            
            // 添加：自动开启对话框
            if (!isDialogueActive && !hasDialogueBeenShown)
            {
                StartDialogue();
            }
        });
    }

    // 移动玩家到Gold旁边
    private void MovePlayerToGold()
    {
        if (player == null) return;

        // 计算目标位置（停在Gold前方一段距离）
        Vector3 targetPosition = transform.position - (transform.position - player.transform.position).normalized * stoppingDistance;

        // 计算移动距离
        float distance = Vector3.Distance(player.transform.position, targetPosition);

        // 计算移动时间
        float moveDuration = distance / moveSpeed;

        // 设置玩家朝向Gold
        Vector3 direction = (targetPosition - player.transform.position).normalized;
        if (direction.x != 0)
        {
            Vector3 scale = player.transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (direction.x > 0 ? 1 : -1);
            player.transform.localScale = scale;
        }

        // 使用DOTween平滑移动玩家
        moveTween = player.transform.DOMove(targetPosition, moveDuration)
            .SetEase(Ease.Linear)
            .OnComplete(() =>
            {
                // 移动完成后停止跑步动画
                StopRunningAnimation();

                // 标记玩家已到达Gold身边
                playerArrived = true;

                // 结束对话
                EndDialogue();
            });

        isMovingToGold = true;
    }

    // 停止跑步动画
    private void StopRunningAnimation()
    {
        if (playerAnimator != null)
        {
            playerAnimator.SetBool("IsRun", false);
        }
        isMovingToGold = false;
    }

    // 立即完成打字效果
    private void CompleteTypewriter()
    {
        if (typewriterTween != null && typewriterTween.IsActive())
        {
            typewriterTween.Complete();
        }
        else if (currentDialogueIndex < dialogueEntries.Count)
        {
            DialogueEntry currentEntry = dialogueEntries[currentDialogueIndex];
            if (currentEntry.isGold)
            {
                goldText.text = currentEntry.text;
            }
            else
            {
                playerText.text = currentEntry.text;
            }
        }
    }

    // 结束对话
    private void EndDialogue()
    {
        // 关闭所有对话框
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }
        if (PlayerDialogueBox != null)
        {
            PlayerDialogueBox.SetActive(false);
        }

        // 隐藏所有头像和文本框
        if (goldImage != null) goldImage.SetActive(false);
        if (goldText != null) goldText.gameObject.SetActive(false);
        if (playerText != null) playerText.gameObject.SetActive(false);

        isDialogueActive = false;
        isDialogueComplete = true;
        hasDialogueBeenShown = true;

        currentDialogueIndex = 0;
    }

    private void OnDisable()
    {
        if (typewriterTween != null && typewriterTween.IsActive())
            typewriterTween.Kill();

        if (moveTween != null && moveTween.IsActive())
        {
            moveTween.Kill();
            StopRunningAnimation();
        }
    }

    private void OnDestroy()
    {
        if (typewriterTween != null && typewriterTween.IsActive())
            typewriterTween.Kill();

        if (moveTween != null && moveTween.IsActive())
        {
            moveTween.Kill();
            StopRunningAnimation();
        }
    }
}