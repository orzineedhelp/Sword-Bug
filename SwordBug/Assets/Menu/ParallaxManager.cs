using UnityEngine;
using System.Collections.Generic;

public class ParallaxManager : MonoBehaviour
{
    [System.Serializable]
    public class ParallaxLayerData
    {
        public string layerName;
        public Transform layerTransform;
        public ParallaxLayer parallaxScript;
        public float parallaxFactor;
        public bool isActive = true;
    }

    [Header("Parallax Configuration")]
    public List<ParallaxLayerData> parallaxLayers = new List<ParallaxLayerData>();

    [Header("Global Settings")]
    public bool enableParallax = true;
    public float globalParallaxMultiplier = 1.0f;

    private Transform cameraTransform;

    void Start()
    {
        cameraTransform = Camera.main.transform;
        InitializeLayers();
    }

    void InitializeLayers()
    {
        // 自动查找所有子对象中的ParallaxLayer组件
        ParallaxLayer[] foundLayers = GetComponentsInChildren<ParallaxLayer>();

        foreach (ParallaxLayer layer in foundLayers)
        {
            ParallaxLayerData layerData = new ParallaxLayerData
            {
                layerName = layer.gameObject.name,
                layerTransform = layer.transform,
                parallaxScript = layer,
                parallaxFactor = layer.parallaxFactor,
                isActive = true
            };

            parallaxLayers.Add(layerData);
            Debug.Log($"Added parallax layer: {layer.gameObject.name}");
        }

        Debug.Log($"Parallax manager initialized with {parallaxLayers.Count} layers");
    }

    void Update()
    {
        if (!enableParallax) return;

        // 更新全局乘数
        UpdateGlobalParallaxMultiplier();
    }

    void UpdateGlobalParallaxMultiplier()
    {
        foreach (ParallaxLayerData layerData in parallaxLayers)
        {
            if (layerData.isActive && layerData.parallaxScript != null)
            {
                // 这里可以添加全局控制逻辑
                // 例如根据游戏状态调整所有层的视差强度
            }
        }
    }

    // 公共方法：启用/禁用特定层
    public void SetLayerActive(string layerName, bool active)
    {
        ParallaxLayerData layer = parallaxLayers.Find(l => l.layerName == layerName);
        if (layer != null)
        {
            layer.isActive = active;
            if (layer.parallaxScript != null)
            {
                layer.parallaxScript.enabled = active;
            }
        }
    }

    // 公共方法：调整所有层的视差强度
    public void SetGlobalParallaxMultiplier(float multiplier)
    {
        globalParallaxMultiplier = multiplier;
        foreach (ParallaxLayerData layerData in parallaxLayers)
        {
            if (layerData.parallaxScript != null)
            {
                layerData.parallaxScript.parallaxFactor = layerData.parallaxFactor * multiplier;
            }
        }
    }

    // 调试方法：重置所有层位置
    public void ResetAllLayers()
    {
        foreach (ParallaxLayerData layerData in parallaxLayers)
        {
            if (layerData.parallaxScript != null)
            {
                layerData.parallaxScript.ResetPosition();
            }
        }
    }
}