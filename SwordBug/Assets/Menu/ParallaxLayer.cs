using UnityEngine;

public class ParallaxLayer : MonoBehaviour
{
    [Header("Parallax Settings")]
    public float parallaxFactor = 0.5f;
    public bool invertParallax = false;
    public bool enableVerticalParallax = true;

    private Vector3 initialPosition;
    private Transform cameraTransform;
    private Vector3 lastCameraRotation;

    void Start()
    {
        // 保存初始位置
        initialPosition = transform.position;

        // 获取摄像机引用
        cameraTransform = Camera.main.transform;
        lastCameraRotation = cameraTransform.eulerAngles;

        Debug.Log($"Parallax layer {gameObject.name} initialized with factor {parallaxFactor}");
    }

    void Update()
    {
        // 获取当前摄像机旋转
        Vector3 currentRotation = cameraTransform.eulerAngles;

        // 计算旋转差异（处理角度环绕）
        Vector3 rotationDelta = CalculateRotationDelta(currentRotation, lastCameraRotation);

        // 计算视差偏移
        float parallaxMultiplier = invertParallax ? -1f : 1f;
        Vector3 parallaxOffset = CalculateParallaxOffset(rotationDelta, parallaxMultiplier);

        // 应用新位置
        transform.position = initialPosition + parallaxOffset;

        // 保存当前旋转供下一帧使用
        lastCameraRotation = currentRotation;
    }

    Vector3 CalculateRotationDelta(Vector3 current, Vector3 last)
    {
        float deltaX = Mathf.DeltaAngle(last.x, current.x);
        float deltaY = Mathf.DeltaAngle(last.y, current.y);
        float deltaZ = Mathf.DeltaAngle(last.z, current.z);

        return new Vector3(deltaX, deltaY, deltaZ);
    }

    Vector3 CalculateParallaxOffset(Vector3 rotationDelta, float multiplier)
    {
        float offsetX = rotationDelta.y * parallaxFactor * multiplier;
        float offsetY = enableVerticalParallax ? rotationDelta.x * parallaxFactor * multiplier : 0f;

        return new Vector3(offsetX, offsetY, 0f);
    }

    // 重置到初始位置（用于调试）
    public void ResetPosition()
    {
        transform.position = initialPosition;
        lastCameraRotation = cameraTransform.eulerAngles;
    }
}