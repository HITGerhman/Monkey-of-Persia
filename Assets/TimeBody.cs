using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

/// <summary>
/// 管理时间回溯机制的组件。
/// 负责记录物体的位置、旋转、速度和颜色信息，并在回溯时还原这些状态。
/// 同时处理回溯时的视觉特效（后处理）、UI 显示以及音频混音。
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class TimeBody : MonoBehaviour
{
    [Header("回溯设置")]
    [Tooltip("最大可回溯的时间长度（秒）")]
    public float recordTime = 5f;

    [Header("特效设置")]
    [Tooltip("回溯时激活的后处理体积")]
    public PostProcessVolume rewindVolume;
    [Tooltip("特效过渡速度")]
    public float effectTransitionSpeed = 5f;

    [Header("UI 设置")]
    [Tooltip("显示剩余回溯能量的 UI 填充图像")]
    public Image energyBarFill;

    /// <summary>
    /// 记录某一时刻的物体状态结构体
    /// </summary>
    private struct PointInTime
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector2 velocity;
        public float angularVelocity;
        public Color color; // 记录颜色状态

        public PointInTime(Vector3 _pos, Quaternion _rot, Vector2 _vel, float _angVel, Color _col)
        {
            position = _pos;
            rotation = _rot;
            velocity = _vel;
            angularVelocity = _angVel;
            color = _col;
        }
    }

    private List<PointInTime> pointsInTime;
    private Rigidbody2D rb;
    private bool isRewinding = false;
    private PlayerController _playerController;
    private SpriteRenderer _sr;

    // 动态计算的目标权重
    private float _targetWeight = 0f;

    void Start()
    {
        pointsInTime = new List<PointInTime>();
        rb = GetComponent<Rigidbody2D>();
        
        // 获取相关组件引用
        _playerController = GetComponent<PlayerController>();
        _sr = GetComponent<SpriteRenderer>();

        // 初始化后处理特效状态
        if (rewindVolume != null)
        {
            rewindVolume.weight = 0f;
        }
        
        UpdateEnergyUI();
    }

    void Update()
    {
        // 监听回溯按键输入
        if (Input.GetKeyDown(KeyCode.Return)) StartRewind();
        if (Input.GetKeyUp(KeyCode.Return)) StopRewind();

        // 动态插值更新后处理权重
        UpdatePostProcessing();

        UpdateEnergyUI();
    }

    void FixedUpdate()
    {
        if (isRewinding) Rewind();
        else Record();
    }

    /// <summary>
    /// 记录当前帧的状态
    /// </summary>
    void Record()
    {
        // 获取当前颜色
        Color currentColor = _sr != null ? _sr.color : Color.white;

        // 在列表头部插入当前状态
        pointsInTime.Insert(0, new PointInTime(
            transform.position, 
            transform.rotation, 
            rb.velocity, 
            rb.angularVelocity,
            currentColor
        ));
        
        // 移除超出最大记录时间的旧数据
        if (pointsInTime.Count > Mathf.Round(recordTime / Time.fixedDeltaTime))
        {
            pointsInTime.RemoveAt(pointsInTime.Count - 1);
        }
    }

    /// <summary>
    /// 执行回溯逻辑
    /// </summary>
    void Rewind()
    {
        if (pointsInTime.Count > 0)
        {
            // 取出最近的状态并应用
            PointInTime point = pointsInTime[0];
            transform.position = point.position;
            transform.rotation = point.rotation;
            
            // 还原颜色
            if (_sr != null)
            {
                _sr.color = point.color;
            }

            pointsInTime.RemoveAt(0);
        }
        else
        {
            // 没有更多记录数据时停止回溯
            StopRewind();
        }
    }

    /// <summary>
    /// 更新能量条 UI 显示
    /// </summary>
    void UpdateEnergyUI()
    {
        if (energyBarFill != null)
        {
            float currentFrames = pointsInTime.Count;
            float maxFrames = recordTime / Time.fixedDeltaTime;
            energyBarFill.fillAmount = currentFrames / maxFrames;
        }
    }

    /// <summary>
    /// 动态更新后处理权重
    /// </summary>
    void UpdatePostProcessing()
    {
        if (rewindVolume != null)
        {
            // 使用 Lerp 实现权重的平滑过渡
            // 这会间接控制色差和畸变的强度，因为它们都绑定在这个 Profile 上
            rewindVolume.weight = Mathf.Lerp(rewindVolume.weight, _targetWeight, Time.deltaTime * effectTransitionSpeed);
        }
    }

    public void StartRewind()
    {
        isRewinding = true;
    
        // 如果关联了玩家控制器，尝试触发复活逻辑（解除物理锁定等）
        if (_playerController != null)
        {
            // 传入 false 表示不强制重置颜色，而是交给 rewind 逐帧还原
            _playerController.Resurrect(false);
        }

        // 关键逻辑：回溯时设为运动学刚体
        // 这解决了物理碰撞的 Edge Case，防止回溯时物体与墙壁发生物理穿插和抖动
        rb.isKinematic = true;

        // 设置目标权重为 1，Update 中会平滑过渡
        _targetWeight = 1f;

        // 播放倒带音效并进行混音
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StartRewindAudio();
        }
    }

    public void StopRewind()
    {
        isRewinding = false;
        
        // 恢复物理模拟
        rb.isKinematic = false;

        // 恢复回溯结束时的速度状态
        if (pointsInTime.Count > 0)
        {
            PointInTime point = pointsInTime[0];
            rb.velocity = point.velocity;
            rb.angularVelocity = point.angularVelocity;
        }

        // 设置目标权重为 0，Update 中会平滑过渡
        _targetWeight = 0f;

        // 停止倒带音效并恢复背景音乐
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopRewindAudio();
        }
    }
}
