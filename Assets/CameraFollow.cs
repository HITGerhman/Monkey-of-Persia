using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("目标设置")]
    public Transform target;        // 要跟随的主角
    public Vector3 offset = new Vector3(0f, 0f, -10f); // 相对偏移量 (Z轴必须是-10，否则相机会看不到2D物体)

    [Header("手感微调")]
    [Range(0, 1)]
    public float smoothSpeed = 0.125f; // 跟随延迟 (0=瞬间, 1=不动)，越小越紧，越大越平滑

    // 内部变量：记录当前速度，供 SmoothDamp 函数使用
    private Vector3 velocity = Vector3.zero;

    // ★ 面试考点：为什么要用 LateUpdate？
    // Update: 处理输入
    // FixedUpdate: 处理物理移动 (主角是在这里动的)
    // LateUpdate: 所有的 Update 执行完后执行。
    // 必须在 LateUpdate 移动相机，才能保证主角已经移动完毕，否则相机会在主角移动前这一帧渲染，导致画面抖动 (Jitter)。
    void LateUpdate()
    {
        if (target == null) return;

        // 1. 计算目标位置 (主角位置 + 偏移)
        Vector3 desiredPosition = target.position + offset;

        // 2. 使用平滑阻尼算法移动相机
        // SmoothDamp 是实现“摄影师扛着机器跑”那种平滑感的在神器
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);

        // 3. 应用位置
        transform.position = smoothedPosition;
    }
}