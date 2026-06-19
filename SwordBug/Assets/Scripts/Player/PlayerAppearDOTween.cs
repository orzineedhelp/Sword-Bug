using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//玩家旋转掉落出场
public class PlayerAppearDOTween : MonoBehaviour
{

    [Header("目标位置")]
    public Transform targetPosition;        // 地面位置

    [Header("移动参数")]
    public float moveDuration = 1.5f;       // 移动持续时间
    public float rotationDuration = 1.5f;   // 旋转持续时间
    public float rotationAngle = 720f;      // 总旋转角度（度）

    [Header("延迟")]
    public float delayBeforeStart = 1f;     // 延迟开始时间

    [Header("音效")]
    public AudioDefination appearSound;     // 登场音效（可选）

    [Header("完成回调")]
    public UnityEngine.Events.UnityEvent onArrived; // 到达时触发的事件（可在Inspector中绑定）
    [Header("开场动画物品")]
    public GameObject appearLight;

    private bool hasArrived = false;

    private Rigidbody2D rb2d;
    void Start()
    {
    }

    private void OnEnable()
    {

        // 一开始禁用玩家控制
        var control = GetComponent<PlayerControl>();
        if (control) control.enabled = false;
        // 隐藏玩家
        var sr = GetComponent<SpriteRenderer>();
        if (sr) sr.enabled = false;
        rb2d = GetComponent<Rigidbody2D>();
        // 禁用重力（禁用物理模拟）
        if (rb2d != null)
        {
            rb2d.simulated = false;        // 完全禁用物理，包括重力
        }
        // 开始登场序列
        StartCoroutine(StartAppearSequence());
    }

    IEnumerator StartAppearSequence()
    {
        
        // 等待延迟
        yield return new WaitForSeconds(delayBeforeStart);
        appearLight.SetActive(true);
        // 恢复玩家
        var sr = GetComponent<SpriteRenderer>();
        if (sr) sr.enabled = true;
        // 播放音效
        if (appearSound != null) appearSound.PlayAudioClip();


        // 创建 DOTween 序列
        Sequence sequence = DOTween.Sequence();

        // 添加移动动画：从当前位置移动到目标位置，使用缓动函数
        sequence.Append(
            transform.DOMove(targetPosition.position, moveDuration)
                .SetEase(Ease.OutQuad)  // 缓出，先快后慢，更自然
        );

        // 同时进行旋转动画（并行）
        sequence.Join(
            transform.DORotate(new Vector3(0, 0, rotationAngle), rotationDuration, RotateMode.FastBeyond360)
                .SetEase(Ease.Linear)   // 匀速旋转
        );

        // 在序列完成后执行回调
        sequence.OnComplete(() =>
        {
            // 精确对齐位置（防止微小误差）
            transform.position = targetPosition.position;
            // 重置旋转到初始值（如果需要）
            transform.rotation = Quaternion.identity;

            // 恢复玩家控制
            var control = GetComponent<PlayerControl>();
            if (control) control.enabled = true;
            //恢复物理
            rb2d.simulated = true;
            
            // 触发自定义事件
            onArrived?.Invoke();

            hasArrived = true;
        });

        // 启动序列
        sequence.Play();
    }

    // 如果需要外部强制终止或重置，可以提供公共方法
    public void ForceStop()
    {
        DOTween.Kill(transform); // 停止所有与 transform 相关的动画
        hasArrived = false;
    }

    
}
