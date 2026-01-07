using UnityEngine;

/// <summary>
/// 相机跟随脚本。
/// 使相机平滑地跟随目标物体（通常是玩家）。
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("目标设置")]
    [Tooltip("要跟随的目标对象")]
    public Transform target;
    [Tooltip("相机相对于目标的偏移量（Z轴通常设置为-10以保证2D物体可见）")]
    public Vector3 offset = new Vector3(0f, 0f, -10f);

    [Header("手感微调")]
    [Range(0, 1)]
    [Tooltip("跟随平滑度：值越小跟随越紧密，值越大越平滑")]
    public float smoothSpeed = 0.125f;

    // SmoothDamp 函数使用的当前速度引用
    private Vector3 velocity = Vector3.zero;

    // 使用 LateUpdate 确保在目标物体所有移动逻辑（Update/FixedUpdate）完成后才移动相机，
    // 避免因执行顺序问题导致画面抖动。
    void LateUpdate()
    {
        if (target == null) return;

        // 计算目标位置
        Vector3 desiredPosition = target.position + offset;

        // 使用平滑阻尼算法移动相机，实现平滑跟随效果
        Vector3 smoothedPosition = Vector3.SmoothDamp(transform.position, desiredPosition, ref velocity, smoothSpeed);

        // 更新相机位置
        transform.position = smoothedPosition;
    }
}