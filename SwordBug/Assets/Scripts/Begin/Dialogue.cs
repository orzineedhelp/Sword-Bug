using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class Dialogue : MonoBehaviour
{
    [Header("UI引用")]
    public GameObject dialogueBox;
    public GameObject playerDialogueBox;
    public TextMeshProUGUI npcText;
    public TextMeshProUGUI playerText;

    [Header("角色头像GameObject")]
    public GameObject npcFaceObject;
    public GameObject playerFaceObject;

    [Header("图标")]
    public GameObject angryIcon;
    //public GameObject cryIcon;
    public Animator animator; // 可以通过代码查找，也可以手动拖拽
    //public Animator animator2;

    [Header("Gold物体引用")]
    public GameObject goldObject; // 添加对Gold物体的引用

    [Header("对话内容")]
    public List<DialogueEntry> dialogueEntries = new List<DialogueEntry>();

    [Header("打字机效果设置")]
    public float typingSpeed = 0.05f;
    public float startDelay = 0.2f;

    [Header("抖动效果设置")]
    public bool enableShakeEffect = true;
    public float baseShakeIntensity = 1f;
    public float shakeIntensityIncrement = 0.5f;

    // 状态变量
    private bool playerNpc;
    private Tween typewriterTween;
    private int currentDialogueIndex = 0;
    private DialogueEffects dialogueEffects;
    private bool isDialogueComplete = false;
    private bool waitingForAngerIcon = false;

    [System.Serializable]
    public class DialogueEntry
    {
        public bool isNPC;
        public string text;
        public bool triggerShake = false;
    }

    void Start()
    {
        // 查找 Angry 物体的 Animator 组件
        FindAngryAnimator();

        // 获取抖动效果组件
        if (dialogueBox != null)
        {
            dialogueEffects = dialogueBox.GetComponent<DialogueEffects>();
        }

        // 自动设置从Element 3开始抖动
        AutoSetShakeTriggers();
    }

    // 查找 Angry 物体的 Animator 组件
    private void FindAngryAnimator()
    {
        // 如果已经在 Inspector 中手动赋值，则不需要查找
        if (animator != null)
        {
            Debug.Log("Animator 已通过 Inspector 赋值: " + animator.gameObject.name);
            return;
        }

        // 通过精确名称查找
        GameObject angryObject = GameObject.Find("Angry");

        // 如果找到了物体，尝试获取 Animator 组件
        if (angryObject != null)
        {
            animator = angryObject.GetComponent<Animator>();
        }

    }

    // 检查 Animator 是否有指定参数
    private bool HasParameter(string paramName, Animator animator)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }

    // 自动设置抖动触发
    private void AutoSetShakeTriggers()
    {
        for (int i = 0; i < dialogueEntries.Count; i++)
        {
            // 从Element 3（索引2）开始设置抖动
            if (i >= 2)
            {
                dialogueEntries[i].triggerShake = true;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player") && currentDialogueIndex == 0)
        {
            // 检查Gold的旋转角度是否为-1.587
            if (IsGoldRotationCorrect())
            {
                StartDialogue();
            }
        }
    }

    // 检查Gold物体的旋转角度是否为-1.587
    private bool IsGoldRotationCorrect()
    {
        if (goldObject == null)
        {
            Debug.LogWarning("Gold物体未赋值，无法检查旋转角度");
            return false;
        }

        // 获取Gold的Z轴旋转角度
        float goldRotationZ = goldObject.transform.rotation.eulerAngles.z;

        // 由于Unity使用0-360度表示，而-1.587度相当于358.413度
        // 我们使用一个小的容差范围来检查
        float targetRotation = 358.413f; // -1.587度对应的0-360度表示
        float tolerance = 0.5f; // 容差范围

        // 检查是否在目标角度附近
        bool isCorrect = Mathf.Abs(goldRotationZ - targetRotation) < tolerance;

        Debug.Log($"Gold旋转角度: {goldRotationZ}, 目标角度: {targetRotation}, 是否符合条件: {isCorrect}");

        return isCorrect;
    }

    // 可选的替代方法：直接比较四元数
    private bool IsGoldRotationCorrectAlternative()
    {
        if (goldObject == null) return false;

        // 创建目标旋转（-1.587度绕Z轴）
        Quaternion targetRotation = Quaternion.Euler(0, 0, -1.587f);

        // 比较两个四元数的角度差
        float angleDifference = Quaternion.Angle(goldObject.transform.rotation, targetRotation);

        // 如果角度差小于阈值，则认为匹配
        bool isCorrect = angleDifference < 1f; // 1度容差

        Debug.Log($"Gold旋转角度差: {angleDifference}, 是否符合条件: {isCorrect}");

        return isCorrect;
    }

    /// <summary>
    /// 对话开始的准备工作
    /// </summary>
    private void StartDialogue()
    {
        if (dialogueBox != null && npcText != null && playerText != null && dialogueEntries.Count > 0)
        {
            playerNpc = true;
            isDialogueComplete = false;
            waitingForAngerIcon = false;
            dialogueBox.SetActive(true);
            ShowCurrentDialogue();
        }
    }
    /// <summary>
    /// 
    /// </summary>
    private void ShowCurrentDialogue()
    {
        if (currentDialogueIndex >= dialogueEntries.Count)
        {
            // 所有对话已结束，等待点击显示生气图标
            isDialogueComplete = true;
            waitingForAngerIcon = true;
            return;
        }

        DialogueEntry currentEntry = dialogueEntries[currentDialogueIndex];

        // 根据说话者设置UI
        if (currentEntry.isNPC)
        {
            npcText.gameObject.SetActive(true);
            playerText.gameObject.SetActive(false);
            playerDialogueBox.SetActive(false);
            if (npcFaceObject != null) npcFaceObject.SetActive(true);
           // if (playerFaceObject != null) playerFaceObject.SetActive(false);//将玩家表情删去
            StartTypewriterEffect(currentEntry.text, npcText);//开启打字机
        }
        else
        {//此时为玩家发言
            playerText.gameObject.SetActive(true);
            playerDialogueBox.SetActive(true);
            npcText.gameObject.SetActive(false);
            dialogueBox.SetActive(false);//玩家有自己的对话框
          //  if (playerFaceObject != null) playerFaceObject.SetActive(true);
            if (npcFaceObject != null) npcFaceObject.SetActive(false);
            StartTypewriterEffect(currentEntry.text, playerText);
        }

        // 检查是否触发抖动效果
        if (enableShakeEffect && currentEntry.triggerShake && dialogueEffects != null)
        {
            int shakeIndex = currentDialogueIndex - 2;
            float intensity = baseShakeIntensity + (shakeIndex * shakeIntensityIncrement);
            StartCoroutine(dialogueEffects.ShakeDialogueBox(intensity));
        }
    }

    /// <summary>
    /// 开始打字效果
    /// </summary>
    /// <param name="text"></param>
    /// <param name="targetText"></param>
    private void StartTypewriterEffect(string text, TextMeshProUGUI targetText)
    {
        if (typewriterTween != null && typewriterTween.IsActive())
            typewriterTween.Kill();

        targetText.text = "";
        float duration = text.Length * typingSpeed;

        //typewriterTween = targetText.DOText(text, duration)
        //    .SetDelay(startDelay)
        //    .SetEase(Ease.Linear);
        // 使用 DOTween.To 方法实现打字机效果
        int charCount = 0;

        typewriterTween = DOTween.To(
            () => charCount, // 获取当前值
            x => {
                charCount = x;
                targetText.text = text.Substring(0, charCount); // 更新文本内容
            },
            text.Length, // 目标值（文本长度）
            duration // 持续时间
        )
        .SetDelay(startDelay)
        .SetEase(Ease.Linear);
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && playerNpc)
        {
            HandleMouseClick();
        }
    }

    private void HandleMouseClick()
    {
        if (waitingForAngerIcon)
        {
            ShowAngerIcon();
            return;
        }

        if (isDialogueComplete)
        {
            waitingForAngerIcon = true;
            return;
        }

        if (typewriterTween != null && typewriterTween.IsActive())
        {
            CompleteTypewriter();
        }
        else
        {
            currentDialogueIndex++;
            ShowCurrentDialogue();
        }
    }

    // 显示生气图标并播放动画
    private void ShowAngerIcon()
    {
        // 隐藏对话框
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }
        angryIcon.SetActive(true);


        if (animator != null)
        {
            animator.SetBool("IsAngry", true);
            Debug.Log("已触发 Angry 物体的动画");

            // 启动协程，1秒后停止动画
            StartCoroutine(StopAngerAnimationAfterDelay(1f));
        }
        else
        {
            Debug.LogError("无法播放动画：Animator 为 null！");
        }

        // 重置状态
        playerNpc = false;
        waitingForAngerIcon = false;
    }

    // 在指定延迟后停止愤怒动画
    private IEnumerator StopAngerAnimationAfterDelay(float delay)
    {
        // 等待指定时间
        yield return new WaitForSeconds(delay);

        // 停止愤怒动画
        if (animator != null)
        {
            animator.SetBool("IsAngry", false);
            Debug.Log("已停止 Angry 物体的动画");
        }

        // 隐藏生气图标
        if (angryIcon != null)
        {
            angryIcon.SetActive(false);
        }

        // 完全结束对话
        EndDialogue();
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
            if (currentEntry.isNPC)
            {
                npcText.text = currentEntry.text;
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
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }

        // 隐藏所有头像和文本框
        if (npcFaceObject != null) npcFaceObject.SetActive(false);
        if (playerFaceObject != null) playerFaceObject.SetActive(false);
        if (npcText != null) npcText.gameObject.SetActive(false);
        if (playerText != null) playerText.gameObject.SetActive(false);

        playerNpc = false;
        currentDialogueIndex = 0;
        isDialogueComplete = false;
        waitingForAngerIcon = false;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            EndDialogue();
        }
    }

    private void OnDisable()
    {
        if (typewriterTween != null && typewriterTween.IsActive())
            typewriterTween.Kill();
    }

    private void OnDestroy()
    {
        if (typewriterTween != null && typewriterTween.IsActive())
            typewriterTween.Kill();
    }
}