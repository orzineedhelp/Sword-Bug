using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ParticleTextSizeKeeper : MonoBehaviour
{
    [Header("Size Preservation Settings")]
    [Tooltip("是否在最大尺寸时冻结粒子")]
    public bool freezeAtMaxSize = true;

    [Tooltip("检查粒子状态的间隔时间")]
    public float checkInterval = 0.1f;

    [Tooltip("等待文本完全展开的时间")]
    public float maxSizeWaitTime = 1.5f;

    [Header("Playback Speed Control")]
    [Tooltip("粒子系统播放速度倍率，大于1加快，小于1减慢")]
    public float playbackSpeed = 1.0f;

    [Header("Debug")]
    [Tooltip("启用调试信息")]
    public bool enableDebug = false;

    private ParticleSystem[] particleSystems;
    private bool isFrozen = false;
    private float startTime;
    private Coroutine freezeCoroutine;

    void Start()
    {
        startTime = Time.time;

        // 应用播放速度
        ApplyPlaybackSpeed();

        if (freezeAtMaxSize)
        {
            freezeCoroutine = StartCoroutine(WaitAndFreezeAtMaxSize());
        }
    }

    // 应用播放速度到所有粒子系统
    void ApplyPlaybackSpeed()
    {
        particleSystems = GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem ps in particleSystems)
        {
            if (ps != null)
            {
                var main = ps.main;
                main.simulationSpeed = playbackSpeed;
            }
        }

        if (enableDebug) Debug.Log($"应用播放速度: {playbackSpeed}x");
    }

    IEnumerator WaitAndFreezeAtMaxSize()
    {
        if (enableDebug) Debug.Log("开始等待文本完全展开...");

        // 根据播放速度调整等待时间
        float adjustedWaitTime = maxSizeWaitTime / playbackSpeed;

        // 等待文本完全展开
        yield return new WaitForSeconds(adjustedWaitTime);

        if (enableDebug) Debug.Log("文本展开完成，开始冻结...");

        // 获取所有粒子系统
        particleSystems = GetComponentsInChildren<ParticleSystem>();

        if (particleSystems == null || particleSystems.Length == 0)
        {
            if (enableDebug) Debug.LogWarning("未找到粒子系统，无法冻结");
            yield break;
        }

        // 冻结所有粒子系统
        FreezeAllParticles();

        isFrozen = true;
        if (enableDebug) Debug.Log("粒子系统已冻结");
    }

    void FreezeAllParticles()
    {
        foreach (ParticleSystem ps in particleSystems)
        {
            if (ps != null && ps.isPlaying)
            {
                // 暂停粒子系统
                ps.Pause();

                // 禁用所有会导致后续变化的模块
                DisableParticleChanges(ps);
            }
        }
    }

    void DisableParticleChanges(ParticleSystem ps)
    {
        // 禁用大小变化
        var sizeOverLifetime = ps.sizeOverLifetime;
        if (sizeOverLifetime.enabled)
        {
            sizeOverLifetime.enabled = false;
            if (enableDebug) Debug.Log($"已禁用 {ps.name} 的 Size Over Lifetime");
        }

        // 禁用颜色变化（特别是透明度）
        var colorOverLifetime = ps.colorOverLifetime;
        if (colorOverLifetime.enabled)
        {
            colorOverLifetime.enabled = false;
            if (enableDebug) Debug.Log($"已禁用 {ps.name} 的 Color Over Lifetime");
        }

        // 禁用速度变化
        var velocityOverLifetime = ps.velocityOverLifetime;
        if (velocityOverLifetime.enabled)
        {
            velocityOverLifetime.enabled = false;
            if (enableDebug) Debug.Log($"已禁用 {ps.name} 的 Velocity Over Lifetime");
        }

        // 禁用限制模块
        var limitVelocityOverLifetime = ps.limitVelocityOverLifetime;
        if (limitVelocityOverLifetime.enabled)
        {
            limitVelocityOverLifetime.enabled = false;
            if (enableDebug) Debug.Log($"已禁用 {ps.name} 的 Limit Velocity Over Lifetime");
        }

        // 禁用力场
        var forceOverLifetime = ps.forceOverLifetime;
        if (forceOverLifetime.enabled)
        {
            forceOverLifetime.enabled = false;
            if (enableDebug) Debug.Log($"已禁用 {ps.name} 的 Force Over Lifetime");
        }

        // 禁用噪声
        var noise = ps.noise;
        if (noise.enabled)
        {
            noise.enabled = false;
            if (enableDebug) Debug.Log($"已禁用 {ps.name} 的 Noise");
        }

        // 禁用旋转随时间变化
        var rotationOverLifetime = ps.rotationOverLifetime;
        if (rotationOverLifetime.enabled)
        {
            rotationOverLifetime.enabled = false;
            if (enableDebug) Debug.Log($"已禁用 {ps.name} 的 Rotation Over Lifetime");
        }

        // 禁用外部力
        var externalForces = ps.externalForces;
        if (externalForces.enabled)
        {
            externalForces.enabled = false;
            if (enableDebug) Debug.Log($"已禁用 {ps.name} 的 External Forces");
        }

        // 设置主模块
        var main = ps.main;
        main.scalingMode = ParticleSystemScalingMode.Local;
        main.loop = false;
    }

    // 手动触发冻结（可以在动画事件中调用）
    public void FreezeNow()
    {
        if (!isFrozen)
        {
            if (enableDebug) Debug.Log("手动触发冻结");

            if (freezeCoroutine != null)
            {
                StopCoroutine(freezeCoroutine);
            }

            particleSystems = GetComponentsInChildren<ParticleSystem>();
            FreezeAllParticles();
            isFrozen = true;
        }
    }

    // 解冻粒子系统（如果需要重新播放）
    public void Unfreeze()
    {
        if (isFrozen)
        {
            if (enableDebug) Debug.Log("解冻粒子系统");

            foreach (ParticleSystem ps in particleSystems)
            {
                if (ps != null)
                {
                    ps.Play();
                }
            }
            isFrozen = false;

            // 重新开始冻结协程
            if (freezeAtMaxSize)
            {
                freezeCoroutine = StartCoroutine(WaitAndFreezeAtMaxSize());
            }
        }
    }

    // 完全重置粒子系统（清除所有粒子并重新开始）
    public void ResetAndPlay()
    {
        if (enableDebug) Debug.Log("重置并重新播放粒子系统");

        // 停止所有协程
        if (freezeCoroutine != null)
        {
            StopCoroutine(freezeCoroutine);
        }

        // 获取所有粒子系统
        particleSystems = GetComponentsInChildren<ParticleSystem>();

        foreach (ParticleSystem ps in particleSystems)
        {
            if (ps != null)
            {
                // 清除现有粒子
                ps.Clear();

                // 重新播放
                ps.Play();
            }
        }

        isFrozen = false;

        // 重新开始冻结协程
        if (freezeAtMaxSize)
        {
            freezeCoroutine = StartCoroutine(WaitAndFreezeAtMaxSize());
        }
    }

    // 设置播放速度
    public void SetPlaybackSpeed(float speed)
    {
        playbackSpeed = speed;
        ApplyPlaybackSpeed();

        // 如果正在等待冻结，重新计算等待时间
        if (!isFrozen && freezeAtMaxSize)
        {
            if (freezeCoroutine != null)
            {
                StopCoroutine(freezeCoroutine);
            }
            freezeCoroutine = StartCoroutine(WaitAndFreezeAtMaxSize());
        }
    }

    // 当文本更新时调用此方法
    public void OnTextUpdated()
    {
        if (enableDebug) Debug.Log("文本已更新，重置冻结状态");

        // 重置状态
        isFrozen = false;
        startTime = Time.time;

        // 应用当前播放速度
        ApplyPlaybackSpeed();

        // 重新开始等待冻结的协程
        if (freezeAtMaxSize)
        {
            if (freezeCoroutine != null)
            {
                StopCoroutine(freezeCoroutine);
            }
            freezeCoroutine = StartCoroutine(WaitAndFreezeAtMaxSize());
        }
    }

    // 检查当前是否已冻结
    public bool IsFrozen()
    {
        return isFrozen;
    }

    // 设置冻结时间
    public void SetFreezeTime(float newFreezeTime)
    {
        maxSizeWaitTime = newFreezeTime;

        // 如果正在等待，重新开始协程
        if (!isFrozen && freezeAtMaxSize)
        {
            if (freezeCoroutine != null)
            {
                StopCoroutine(freezeCoroutine);
            }
            freezeCoroutine = StartCoroutine(WaitAndFreezeAtMaxSize());
        }
    }

    void OnDestroy()
    {
        // 清理协程
        if (freezeCoroutine != null)
        {
            StopCoroutine(freezeCoroutine);
        }
    }
}