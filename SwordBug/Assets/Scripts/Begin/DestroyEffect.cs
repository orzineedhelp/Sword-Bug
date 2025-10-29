using UnityEngine;
using Cinemachine;
using DG.Tweening;
using TMPro;
using UnityEngine.Rendering;

public class DestroyEffect : MonoBehaviour
{
    [Header("物体引用")]
    public GameObject object1; // 物体1（将被监视的物体）
    public GameObject object2; // 物体2（将被激活的物体）
    public CinemachineVirtualCamera virtualCamera; // Cinemachine虚拟相机
    public AudioSource audioSource;
    public AudioClip clip;
    public Animator animator;
    [Header("对话框设置")]
    public GameObject dialogPanel; // 对话框面板
    [TextArea(3, 5)]
    public string message = "这是要显示的文字"; // 要显示的文字

    [Header("时间设置")]
    public float delayAfterDialog = 1f; // 对话框消失后延迟时间（秒）

    [Header("下降设置")]
    public float descentDistance = 3.86f; // 下降距离
    public float descentDuration = 2f;    // 下降持续时间（秒）

    private bool effectTriggered = false; // 防止重复触发
    private bool waitingForClick = false; // 是否等待点击
    private Vector3 object2StartPosition; // 物体2的起始位置
    private PlayerInputControl playerInputControl;

    private void Awake()
    {
        playerInputControl=new PlayerInputControl();
        audioSource = GetComponent<AudioSource>();
    }
    private void OnEnable()
    {
        playerInputControl.Enable();
    }
    void Start()
    {
        // 确保物体2初始状态为未激活
        if (object2 != null)
        {
            object2.SetActive(false);
            // 记录物体2的初始位置
            object2StartPosition = object2.transform.position;
        }

        // 确保对话框初始状态为未激活
        if (dialogPanel != null)
            dialogPanel.SetActive(false);
    }

    void Update()
    {
        // 检查物体1是否被销毁且效果尚未触发
        if (object1 == null && !effectTriggered)
        {
            effectTriggered = true;
            OnObject1Destroyed();
        }

        // 如果正在等待点击，检测鼠标左键
        if (waitingForClick && playerInputControl.UI.Click.triggered)
        {
            OnMouseClicked();
        }
    }

    void OnObject1Destroyed()
    {
        // 停止镜头抖动
        if (virtualCamera != null)
        {
            var noise = virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            if (noise != null)
            {
                noise.m_AmplitudeGain = 0f;
                noise.m_FrequencyGain = 0f;
            }
        }

        // 显示对话框
        ShowDialog();
        animator.SetBool("IsAngry", true);

    }

    void ShowDialog()
    {
        if (dialogPanel != null)
        {
            dialogPanel.SetActive(true);

            // 查找对话框中的文本组件
            var textComponent = dialogPanel.GetComponentInChildren<TextMeshProUGUI>();
            if (textComponent != null)
            {
                // 清空文本
                textComponent.text = "";

                // 使用 DOTween.To 实现打字机效果
                int charCount = 0;
                float duration = message.Length * 0.05f;

                DOTween.To(
                    () => charCount, // 获取当前值
                    x => {
                        charCount = x;
                        textComponent.text = message.Substring(0, charCount); // 更新文本内容
                    },
                    message.Length, // 目标值（文本长度）
                    duration // 持续时间
                )
                .SetEase(Ease.Linear)
                .OnComplete(() => {
                    // 文字显示完成后，等待点击
                    waitingForClick = true;
                });
            }
            else
            {
                // 如果没有找到文本组件，直接等待点击
                waitingForClick = true;
            }
        }
        else
        {
            // 如果没有对话框，延迟2秒后激活物体2
            Invoke("ActivateObject2", delayAfterDialog);
        }
    }
    void OnMouseClicked()
    {
        waitingForClick = false;

        // 隐藏对话框
        if (dialogPanel != null)
            dialogPanel.SetActive(false);

        // 延迟2秒后激活物体2
        Invoke("ActivateObject2", delayAfterDialog);
    }

    void ActivateObject2()
    {
        if (object2 != null)
        {
            object2.SetActive(true);
            // 开始缓慢下降
            StartDescent();
        }
    }

    void StartDescent()
    {
        // 计算目标位置（Y轴减少指定距离）
        Vector3 targetPosition = object2StartPosition - new Vector3(0, descentDistance, 0);
        audioSource.Play();
        // 使用DOTween实现缓慢下降
        object2.transform.DOMove(targetPosition, descentDuration)
            .SetEase(Ease.OutCubic).OnComplete(() =>
            {
                // 下降完成后直接启动GoldDialogue，不需要玩家点击
                StartGoldDialogue();
            });
    }

    void StartGoldDialogue()
    {
        // 获取GoldDialogue组件并直接启动对话
        GoldDialogue goldDialogue = object2.GetComponent<GoldDialogue>();
        if (goldDialogue != null)
        {
            audioSource.PlayOneShot(clip);
            goldDialogue.StartDialogue();
        }
        else
        {
            Debug.LogWarning("在object2上找不到GoldDialogue组件");
        }
    }
}