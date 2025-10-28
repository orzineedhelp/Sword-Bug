using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.InputSystem;
using TMPro;
using System;

public class DialogueTrigger : MonoBehaviour
{
    [Header("UI引用")]
    public GameObject dialogueBox;
    public GameObject PlayerDialogueBox;
    public TextMeshProUGUI goldText;
    public TextMeshProUGUI playerText;

    [Header("角色头像")]
    public Image goldImage;    // Gold的头像

    [Header("对话内容")]
    public List<DialogueEntry> dialogueEntries = new List<DialogueEntry>();

    [Header("触发对话相关")]
    public bool isDialogue;
    public GameObject player;

    [Header("打字机效果设置")]
    public float typingSpeed = 0.05f;
    public float startDelay = 0.2f;

    [Header("情绪图标设置")]
    public float emotionIconScaleDuration = 0.8f; // 图标缩放动画时长（调慢了）

    // 私有变量
    private bool dialogueActive = false;
    private bool dialogueCompleted = false;
    private int currentDialogueIndex = 0;
    private Tween typewriterTween;
    private bool canProceedToNext = false;
    private GameObject currentEmotionIcon; // 当前显示的情绪图标
    private PlayerInputControl inputActions;



    private void Awake()
    {
        inputActions=new PlayerInputControl();
        player = GameObject.FindWithTag("Player");
    }
    private void OnEnable()
    {
        inputActions.Enable();
    }
    [System.Serializable]
    public class DialogueEntry
    {
        [TextArea(2, 4)]
        public string text;
        public bool isGoldSpeaking = true; // true = Gold说话, false = 玩家说话
        public GameObject emotionIcon; // 直接拖拽情绪图标GameObject，如果为空则不显示
    }

    void Start()
    {
        // 初始隐藏对话框和头像
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
        }
        if (PlayerDialogueBox != null)
        {
            PlayerDialogueBox.SetActive(false);
        }
        if (goldImage != null) goldImage.gameObject.SetActive(false);

        // 隐藏所有文本
        if (goldText != null) goldText.gameObject.SetActive(false);
        if (playerText != null) playerText.gameObject.SetActive(false);

        // 确保所有情绪图标是隐藏的
        EnsureEmotionIconsAreHidden();

       
    }

    // 确保所有情绪图标在场景中是隐藏的
    void EnsureEmotionIconsAreHidden()
    {
        foreach (DialogueEntry entry in dialogueEntries)
        {
            if (entry.emotionIcon != null)
            {
                entry.emotionIcon.SetActive(false);
            }
        }
    }

    void Update()
    {
        //// TODO
        //// 设置条件触发触发对话
        if (isDialogue && !dialogueActive && !dialogueCompleted)
        {
            StartDialogue();
        }

        // 处理对话继续输入（鼠标左键&&enter&&space）
        if (dialogueActive && (inputActions.UI.Click.triggered))
        {
            HandleDialogueInput();
        }
    }
    public void ChangeisDialogue()
    {
        if (!dialogueCompleted)
        {
            isDialogue = true;
        }

    }

    /// <summary>
    /// 开启对话
    /// </summary>
    void StartDialogue()
    {
        if (dialogueEntries.Count == 0) return;
        dialogueActive = true;
        currentDialogueIndex = 0;
        // 暂停游戏
        PauseGame(true);
        // 显示对话框
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(true);
        }

        // 显示第一个对话
        ShowCurrentDialogue();
    }

    private void PauseGame(bool isStop)
    {
        if (isStop)
        {
        //   player.GetComponent<PlayerControl>().enabled = false;
            //暂停输入
            
        }
        else
        {
          //  player.GetComponent<PlayerControl>().enabled = true;
           

        }
    }

    /// <summary>
    /// 显示当前对话
    /// </summary>
    void ShowCurrentDialogue()
    {
        // 隐藏上一句话的情绪图标
        if (currentEmotionIcon != null)
        {
            currentEmotionIcon.SetActive(false);
            currentEmotionIcon = null;
        }

        if (currentDialogueIndex >= dialogueEntries.Count)
        {
            EndDialogue();
            return;
        }

        DialogueEntry currentEntry = dialogueEntries[currentDialogueIndex];

        // 根据谁在说话显示对应的头像和文本框
        if (currentEntry.isGoldSpeaking)
        {
            // Gold说话 - 激活Gold对话框，关闭玩家对话框
            if (dialogueBox != null)
            {
                dialogueBox.SetActive(true);
            }
            if (PlayerDialogueBox != null)
            {
                PlayerDialogueBox.SetActive(false);
            }

            if (goldImage != null)
            {
                goldImage.gameObject.SetActive(true);
            }
            if (goldText != null)
            {
                goldText.gameObject.SetActive(true);
                playerText.gameObject.SetActive(false);
                StartTypewriterEffect(currentEntry.text, goldText);
            }

            
        }
        else
        {
            // 玩家说话 - 激活玩家对话框，关闭Gold对话框
            if (dialogueBox != null)
            {
                dialogueBox.SetActive(false);
            }
            if (PlayerDialogueBox != null)
            {
                PlayerDialogueBox.SetActive(true);
            }

            if (goldImage != null)
            {
                goldImage.gameObject.SetActive(false);
            }

            if (playerText != null)
            {
                playerText.gameObject.SetActive(true);
                goldText.gameObject.SetActive(false);
                StartTypewriterEffect(currentEntry.text, playerText);
            }

           
        }

        canProceedToNext = false;
    }

    /// <summary>
    /// 开启打字机
    /// </summary>
    /// <param name="text"></param>
    /// <param name="targetText"></param>
    void StartTypewriterEffect(string text, TextMeshProUGUI targetText)
    {
        // 清除之前的打字效果
        if (typewriterTween != null && typewriterTween.IsActive())
        {
            typewriterTween.Kill();
        }

        targetText.text = "";
        float duration = text.Length * typingSpeed;

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
        .SetEase(Ease.Linear)
        .OnComplete(() =>
        {
            canProceedToNext = true;
            // 打字完成后显示情绪图标
            ShowEmotionIcon();
        }); 
    }

    /// <summary>
    /// 展示emoji
    /// </summary>
    void ShowEmotionIcon()
    {
        DialogueEntry currentEntry = dialogueEntries[currentDialogueIndex];

        // 检查是否需要显示情绪图标
        if (currentEntry.emotionIcon != null)
        {
            // 直接使用拖拽的GameObject
            currentEmotionIcon = currentEntry.emotionIcon;

            // 确保情绪图标是激活状态
            currentEmotionIcon.SetActive(true);

            // 保存原始缩放值
            Vector3 originalScale = currentEmotionIcon.transform.localScale;

            // 设置初始大小为0
            currentEmotionIcon.transform.localScale = Vector3.zero;

            // 播放由小变大的动画（使用更慢的速度）
            currentEmotionIcon.transform.DOScale(originalScale, emotionIconScaleDuration)
                .SetEase(Ease.OutBack);

            Debug.Log($"显示情绪图标: {currentEmotionIcon.name}");
        }
    }


    void HandleDialogueInput()
    {
        if (!canProceedToNext)
        {
            // 如果打字效果还没完成，立即完成
            CompleteTypewriter();
            return;
        }

        // 继续到下一句对话
        currentDialogueIndex++;
        ShowCurrentDialogue();
    }

    void CompleteTypewriter()
    {
        if (typewriterTween != null && typewriterTween.IsActive())
        {
            typewriterTween.Complete();
        }
        else
        {
            // 手动完成文本显示
            DialogueEntry currentEntry = dialogueEntries[currentDialogueIndex];
            if (currentEntry.isGoldSpeaking)
            {
                goldText.text = currentEntry.text;
            }
            else
            {
                playerText.text = currentEntry.text;
            }

            // 显示情绪图标
            ShowEmotionIcon();
        }
        canProceedToNext = true;
    }

    /// <summary>
    /// 结束对话
    /// </summary>
    void EndDialogue()
    {
        dialogueActive = false;
        dialogueCompleted = true;

        // 隐藏当前情绪图标
        if (currentEmotionIcon != null)
        {
            currentEmotionIcon.SetActive(false);
            currentEmotionIcon = null;
        }

        // 隐藏对话框和头像
        if (dialogueBox != null)
        {
            dialogueBox.SetActive(false);
            PlayerDialogueBox.SetActive(false);
        }
        if (goldImage != null) goldImage.gameObject.SetActive(false);

        // 重置文本
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

        if (dialogueEntries.Count > 0)
        {
            DialogueEntry lastEntry = dialogueEntries[dialogueEntries.Count - 1];
           
        }

        Debug.Log("对话结束");
        isDialogue=false;
        PauseGame(false);
        // 恢复所有游戏操作

    }

    // 公共方法：可以手动触发对话（如果需要的话）
    public void TriggerDialogueManually()
    {
        if (!dialogueActive && !dialogueCompleted)
        {
            StartDialogue();
        }
    }

    // 公共方法：重置对话状态
    public void ResetDialogue()
    {
        dialogueActive = false;
        dialogueCompleted = false;
        currentDialogueIndex = 0;

        // 隐藏当前情绪图标
        if (currentEmotionIcon != null)
        {
            currentEmotionIcon.SetActive(false);
            currentEmotionIcon = null;
        }

        if (typewriterTween != null && typewriterTween.IsActive())
        {
            typewriterTween.Kill();
        }

        
    }

   
}