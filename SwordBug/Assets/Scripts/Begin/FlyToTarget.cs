using UnityEngine;
using System.Collections;

public class FlyToTarget : MonoBehaviour
{
    [Header("导航设置")]
    public GameObject targetObject;
    public float moveSpeed = 5f;
    public float rotationSpeed = 2f;
    public float stoppingDistance = 0.1f;

    [Header("高级选项")]
    public bool useSmoothRotation = true;
    public bool destroyOnArrival = false;
    public bool showDebugInfo = true;

    // 事件委托
    public System.Action OnDestinationReached;

    private bool isMoving = true;

    void Start()
    {
        // 在开始时检查目标是否设置
        if (targetObject == null)
        {
            Debug.LogError("目标物体未设置！请在Inspector中指定目标物体。");
        }
        else
        {
            float initialDistance = Vector2.Distance(transform.position, targetObject.transform.position);
            Debug.Log($"导航系统初始化：{gameObject.name} 将移动到 {targetObject.name}");
            Debug.Log($"初始距离：{initialDistance}");
        }
    }

    void Update()
    {
        if (targetObject == null)
        {
            if (showDebugInfo)
                Debug.LogWarning("目标物体为空，无法移动");
            return;
        }

        if (!isMoving)
        {
            return;
        }

        // 计算当前位置与目标位置的距离（2D）
        Vector2 currentPos = transform.position;
        Vector2 targetPos = targetObject.transform.position;
        float distance = Vector2.Distance(currentPos, targetPos);

        if (showDebugInfo && Time.frameCount % 30 == 0) // 每30帧输出一次，避免日志过多
        {
       //     Debug.Log($"当前距离: {distance}, 移动速度: {moveSpeed}");
        }

        if (distance > stoppingDistance)
        {
            // 方法1：使用MoveTowards（更稳定）
            transform.position = Vector2.MoveTowards(
                currentPos,
                targetPos,
                moveSpeed * Time.deltaTime
            );

            // 方法2：使用方向向量（可选，注释掉方法1并使用这个）
            // Vector2 direction = (targetPos - currentPos).normalized;
            // transform.position += (Vector3)(direction * moveSpeed * Time.deltaTime);

            // 2D 旋转 - 使物体朝向移动方向
            Vector2 direction = (targetPos - currentPos).normalized;
            if (useSmoothRotation)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                Quaternion targetRotation = Quaternion.AngleAxis(angle, Vector3.forward);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
            }
            else
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
            }
        }
        else
        {
            // 到达目的地
            ArriveAtDestination();
        }
    }

    private void ArriveAtDestination()
    {
        isMoving = false;

        // 触发到达事件
        OnDestinationReached?.Invoke();

        Debug.Log($"{gameObject.name} 已到达目标位置！");

        // 可选：到达后销毁物体
        if (destroyOnArrival)
        {
            Destroy(gameObject);
        }
    }

    // 公共方法：设置新目标
    public void SetNewTarget(GameObject newTarget)
    {
        targetObject = newTarget;
        isMoving = true;
        Debug.Log($"新目标设置为：{newTarget.name}");
    }

    // 公共方法：开始移动
    public void StartMoving()
    {
        isMoving = true;
        Debug.Log("开始移动");
    }

    // 公共方法：停止移动
    public void StopMoving()
    {
        isMoving = false;
        Debug.Log("停止移动");
    }

    // 在Scene视图中绘制调试信息
    void OnDrawGizmos()
    {
        if (showDebugInfo && targetObject != null)
        {
            // 绘制从当前物体到目标的线
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, targetObject.transform.position);

            // 在目标位置绘制球体
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(targetObject.transform.position, stoppingDistance);

            // 在当前物体位置绘制箭头
            Gizmos.color = Color.blue;
            Vector3 direction = (targetObject.transform.position - transform.position).normalized;
            Gizmos.DrawRay(transform.position, direction * 1f);
        }
    }
}