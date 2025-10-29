using UnityEngine;

public class MouseLookController : MonoBehaviour
{
    [Header("Mouse Look Settings")]
    public float mouseSensitivity = 2.0f;
    public float verticalClampAngle = 80.0f;

    private float rotationX = 0.0f;
    private float rotationY = 0.0f;

    void Start()
    {
        // 锁定鼠标到屏幕中心
        Cursor.lockState = CursorLockMode.Locked;

        // 获取初始旋转
        Vector3 rot = transform.localRotation.eulerAngles;
        rotationY = rot.y;
        rotationX = rot.x;
    }

    void Update()
    {
        // 获取鼠标输入
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // 计算旋转
        rotationY += mouseX;
        rotationX -= mouseY;

        // 限制垂直旋转角度
        rotationX = Mathf.Clamp(rotationX, -verticalClampAngle, verticalClampAngle);

        // 应用旋转
        Quaternion localRotation = Quaternion.Euler(rotationX, rotationY, 0.0f);
        transform.rotation = localRotation;
    }

    // 可选：按ESC键解锁鼠标
    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }
}