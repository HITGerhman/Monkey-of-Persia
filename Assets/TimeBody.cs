using System.Collections.Generic;
using UnityEngine;
// 【新增】引入后处理命名空间
using UnityEngine.Rendering.PostProcessing;
using UnityEngine.UI; // 【新增】引用 UI 命名空间

[RequireComponent(typeof(Rigidbody2D))]
public class TimeBody : MonoBehaviour
{
    [Header("回溯设置")]
    public float recordTime = 5f;

    // 【新增】用于拖拽我们的特效体积
    [Header("特效设置")]
    public PostProcessVolume rewindVolume;

    [Header("UI 设置")]
    public Image energyBarFill; // 【新增】拖入那个绿色的 Image

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
    private PlayerController _playerController; // 【新增】

    void Start()
    {
        pointsInTime = new List<PointInTime>();
        rb = GetComponent<Rigidbody2D>();

        // 【新增】确保游戏开始时特效是关闭的
        if (rewindVolume != null)
        {
            rewindVolume.weight = 0f;
            _playerController = GetComponent<PlayerController>();
            // 【新增】每帧更新 UI
            UpdateEnergyUI();
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return)) StartRewind();
        if (Input.GetKeyUp(KeyCode.Return)) StopRewind();

        // 【新增】每帧更新 UI
        UpdateEnergyUI();
    }

    void FixedUpdate()
    {
        if (isRewinding) Rewind();
        else Record();
    }

    void Record()
    {
        pointsInTime.Insert(0, new PointInTime(transform.position, transform.rotation, rb.velocity, rb.angularVelocity));
        if (pointsInTime.Count > Mathf.Round(recordTime / Time.fixedDeltaTime))
        {
            pointsInTime.RemoveAt(pointsInTime.Count - 1);
        }
    }

    void Rewind()
    {
        if (pointsInTime.Count > 0)
        {
            PointInTime point = pointsInTime[0];
            transform.position = point.position;
            transform.rotation = point.rotation;
            pointsInTime.RemoveAt(0);
        }
        else
        {
            StopRewind();
        }
    }
    void UpdateEnergyUI()
    {
        if (energyBarFill != null)
        {
            // 计算当前记录了多少帧
            float currentFrames = pointsInTime.Count;
            // 计算最大能记录多少帧 (时间 / 固定帧间隔)
            float maxFrames = recordTime / Time.fixedDeltaTime;

            // 比例 = 当前 / 最大
            energyBarFill.fillAmount = currentFrames / maxFrames;
        }
    }

    public void StartRewind()
    {
        isRewinding = true;
        rb.isKinematic = true;

        // 【新增】开启特效：把权重设为 1
        // 如果想更高级，可以用协程或DOTween平滑过渡，这里先直接切换
        if (rewindVolume != null)
        {
            rewindVolume.weight = 1f;
        }
        // 【新增】复活逻辑
        // 只要开始倒流，我们就假设玩家正在尝试从死亡中恢复
        // 我们调用 Player 的复活方法，先把颜色变回来，状态重置
        if (_playerController != null)
        {
            _playerController.Resurrect();
        }
    }

    public void StopRewind()
    {
        isRewinding = false;
        rb.isKinematic = false;
        if (pointsInTime.Count > 0)
        {
            PointInTime point = pointsInTime[0];
            rb.velocity = point.velocity;
            rb.angularVelocity = point.angularVelocity;
        }

        // 【新增】关闭特效：把权重设为 0
        if (rewindVolume != null)
        {
            rewindVolume.weight = 0f;
        }
    }
}