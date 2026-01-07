using UnityEngine;

/// <summary>
/// 全局音频管理器（单例模式）。
/// 实现跨场景的音效持久化管理，支持背景音乐与音效的独立控制，
/// 特别优化了倒带声效的无缝混音播放。
/// </summary>
public class AudioManager : MonoBehaviour
{
    #region 单例实现

    private static AudioManager _instance;

    /// <summary>
    /// 全局唯一实例访问点
    /// </summary>
    public static AudioManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // 尝试在场景中查找已存在的实例
                _instance = FindObjectOfType<AudioManager>();

                // 如果场景中不存在，则动态创建
                if (_instance == null)
                {
                    GameObject audioManagerObj = new GameObject("AudioManager");
                    _instance = audioManagerObj.AddComponent<AudioManager>();
                }
            }
            return _instance;
        }
    }

    #endregion

    #region 音频资源配置

    [Header("背景音乐")]
    [Tooltip("游戏背景音乐")]
    public AudioClip bgmClip;
    [Range(0f, 1f)]
    public float bgmVolume = 0.5f;

    [Header("音效")]
    [Tooltip("跳跃音效")]
    public AudioClip jumpSfx;
    [Tooltip("死亡音效")]
    public AudioClip dieSfx;
    [Tooltip("胜利音效")]
    public AudioClip winSfx;

    [Header("倒带音效")]
    [Tooltip("倒带循环音效（持续播放）")]
    public AudioClip rewindLoopSfx;
    [Tooltip("倒带开始音效")]
    public AudioClip rewindStartSfx;
    [Tooltip("倒带结束音效")]
    public AudioClip rewindStopSfx;

    [Header("音量设置")]
    [Range(0f, 1f)]
    public float sfxVolume = 1f;
    [Range(0f, 1f)]
    public float rewindVolume = 0.8f;

    #endregion

    #region 音频源组件

    private AudioSource _bgmSource;        // 背景音乐音频源
    private AudioSource _sfxSource;        // 普通音效音频源
    private AudioSource _rewindLoopSource; // 倒带循环音效音频源（独立管理以支持无缝混音）

    #endregion

    #region 生命周期

    private void Awake()
    {
        // 单例模式：确保场景中只有一个实例
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;

        // 跨场景持久化：切换场景时不销毁此对象
        DontDestroyOnLoad(gameObject);

        InitializeAudioSources();
    }

    private void Start()
    {
        PlayBGM();
    }

    #endregion

    #region 初始化

    /// <summary>
    /// 初始化所有音频源组件
    /// </summary>
    private void InitializeAudioSources()
    {
        // 背景音乐音频源
        _bgmSource = gameObject.AddComponent<AudioSource>();
        _bgmSource.loop = true;
        _bgmSource.playOnAwake = false;
        _bgmSource.volume = bgmVolume;

        // 普通音效音频源
        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.loop = false;
        _sfxSource.playOnAwake = false;

        // 倒带循环音效音频源（独立音频源以支持与其他音效同时播放）
        _rewindLoopSource = gameObject.AddComponent<AudioSource>();
        _rewindLoopSource.loop = true;
        _rewindLoopSource.playOnAwake = false;
        _rewindLoopSource.volume = rewindVolume;
    }

    #endregion

    #region 背景音乐控制

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    public void PlayBGM()
    {
        if (bgmClip != null && !_bgmSource.isPlaying)
        {
            _bgmSource.clip = bgmClip;
            _bgmSource.Play();
        }
    }

    /// <summary>
    /// 停止背景音乐
    /// </summary>
    public void StopBGM()
    {
        _bgmSource.Stop();
    }

    /// <summary>
    /// 暂停/恢复背景音乐
    /// </summary>
    public void PauseBGM(bool pause)
    {
        if (pause) _bgmSource.Pause();
        else _bgmSource.UnPause();
    }

    /// <summary>
    /// 设置背景音乐音量
    /// </summary>
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        _bgmSource.volume = bgmVolume;
    }

    #endregion

    #region 音效播放

    /// <summary>
    /// 播放一次性音效
    /// </summary>
    /// <param name="clip">要播放的音频片段</param>
    public void PlaySfx(AudioClip clip)
    {
        if (clip != null)
        {
            _sfxSource.PlayOneShot(clip, sfxVolume);
        }
    }

    /// <summary>
    /// 播放跳跃音效
    /// </summary>
    public void PlayJumpSfx()
    {
        PlaySfx(jumpSfx);
    }

    /// <summary>
    /// 播放死亡音效
    /// </summary>
    public void PlayDieSfx()
    {
        PlaySfx(dieSfx);
    }

    /// <summary>
    /// 播放胜利音效
    /// </summary>
    public void PlayWinSfx()
    {
        PlaySfx(winSfx);
    }

    /// <summary>
    /// 设置音效音量
    /// </summary>
    public void SetSfxVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
    }

    #endregion

    #region 倒带音效控制

    /// <summary>
    /// 开始倒带音效（播放开始音效并启动循环音效）
    /// </summary>
    public void StartRewindAudio()
    {
        // 播放倒带开始音效
        PlaySfx(rewindStartSfx);

        // 启动倒带循环音效
        if (rewindLoopSfx != null && !_rewindLoopSource.isPlaying)
        {
            _rewindLoopSource.clip = rewindLoopSfx;
            _rewindLoopSource.volume = rewindVolume;
            _rewindLoopSource.Play();
        }

        // 降低背景音乐音量，突出倒带效果（混音处理）
        if (_bgmSource != null)
        {
            _bgmSource.volume = bgmVolume * 0.3f;
        }
    }

    /// <summary>
    /// 停止倒带音效（停止循环音效并播放结束音效）
    /// </summary>
    public void StopRewindAudio()
    {
        // 停止倒带循环音效
        if (_rewindLoopSource.isPlaying)
        {
            _rewindLoopSource.Stop();
        }

        // 播放倒带结束音效
        PlaySfx(rewindStopSfx);

        // 恢复背景音乐音量
        if (_bgmSource != null)
        {
            _bgmSource.volume = bgmVolume;
        }
    }

    /// <summary>
    /// 设置倒带音效音量
    /// </summary>
    public void SetRewindVolume(float volume)
    {
        rewindVolume = Mathf.Clamp01(volume);
        _rewindLoopSource.volume = rewindVolume;
    }

    #endregion

    #region 全局音量控制

    /// <summary>
    /// 设置主音量（影响所有音频）
    /// </summary>
    public void SetMasterVolume(float volume)
    {
        AudioListener.volume = Mathf.Clamp01(volume);
    }

    /// <summary>
    /// 静音/取消静音
    /// </summary>
    public void SetMute(bool mute)
    {
        AudioListener.volume = mute ? 0f : 1f;
    }

    #endregion
}
