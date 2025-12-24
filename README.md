# ⏳ Time Rewind Demo (Unity)

一个基于 Unity 2D 的时间回溯机制 Demo，灵感来源于《Braid》和《波斯王子》。
实现了基于环形缓冲区的状态记录与回放系统。

## 🎮 演示 (Demo)

![Gameplay Demo](这里放你的GIF动图链接.gif)
*(建议用 LICEcap 或 ScreenToGif 录制一个 5秒的动图，上传到 GitHub Issue 或图床，然后把链接贴在这里。没有动图的项目是没有灵魂的！)*

## 🛠️ 技术栈 (Tech Stack)

* **Engine**: Unity 2022.3 LTS
* **Language**: C#
* **Patterns**: Command Pattern (Recorder), Object Pooling

## ✨ 核心功能 (Key Features)

* **Time Rewind System**:
    * 使用 `Struct` 记录快照以优化内存 (GC Free)。
    * 实现了位置、旋转、速度、角速度的完整回溯，保证惯性连续性。
    * 支持时间回溯时的物理层 (`isKinematic`) 接管。
* **Polished Controller**:
    * 实现了 Coyote Time (土狼时间) 和 Jump Buffer (跳跃预输入) 以优化手感。
* **Visual Effects**:
    * 集成 Post-processing Stack，实现回溯时的黑白/色差故障风特效。

## 📂 核心代码 (Code Highlight)

核心回溯逻辑位于 `Scripts/TimeBody.cs`：

```csharp
// 这里的代码片段展示你的数据结构设计
private struct PointInTime
{
    public Vector3 position;
    public Quaternion rotation;
    public Vector2 velocity;
    public float angularVelocity;
}
🚀 如何运行 (How to Run)
Clone this repository.

Open with Unity 2022.3.xx.

Open Scene Scenes/SampleScene.

Press Play.

Move: A / D

Jump: Space

Rewind: Hold Enter (Return)
