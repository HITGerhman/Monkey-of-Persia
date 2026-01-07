using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI;

/// <summary>
/// 管理时间回溯机制的组件。
/// 负责记录物体的位置、旋转和速度信息，并在回溯时还原这些状态。
/// 同时处理回溯时的视觉特效（后处理）和 UI 显示。
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

        public PointInTime(Vector3 _pos, Quaternion _rot, Vector2 _vel, float _angVel)
        {
            position = _pos;
            rotation = _rot;
            velocity = _vel;
            angularVelocity = _angVel;
        }
    }

    private List<PointInTime> pointsInTime;
    private Rigidbody2D rb;
    private bool isRewinding = false;
    private PlayerController _playerController;
    private SpriteRenderer _sr;

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
        // 在列表头部插入当前状态
        pointsInTime.Insert(0, new PointInTime(transform.position, transform.rotation, rb.velocity, rb.angularVelocity));
        
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

    public void StartRewind()
    {
        isRewinding = true;
    
        // 如果关联了玩家控制器，尝试触发复活逻辑（解除物理锁定等）
        if (_playerController != null)
        {
            _playerController.Resurrect(false);
        }

        // 回溯时设为运动学刚体，由代码直接控制位置
        rb.isKinematic = true;

        // 启用回溯视觉特效
        if (rewindVolume != null)
        {
            rewindVolume.weight = 1f;
        }
    }

    public void StopRewind()
    {
        isRewinding = false;
        rb.isKinematic = false;

        // 恢复回溯结束时的速度状态
        if (pointsInTime.Count > 0)
        {
            PointInTime point = pointsInTime[0];
            rb.velocity = point.velocity;
            rb.angularVelocity = point.angularVelocity;
        }

        // 关闭回溯视觉特效
        if (rewindVolume != null)
        {
            rewindVolume.weight = 0f;
        }
    }
}