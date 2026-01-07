using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 处理关卡终点逻辑。
/// 当玩家到达终点时，显示胜利 UI 并处理关卡重置。
/// </summary>
public class FinishPoint : MonoBehaviour
{
    [Header("UI 设置")]
    [Tooltip("胜利时显示的 UI 对象")]
    public GameObject winUiObject;

    private bool _levelCompleted = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 如果碰到的是主角，且关卡还没结束
        if (!_levelCompleted && collision.gameObject.CompareTag("Player"))
        {
            CompleteLevel();
        }
    }

    /// <summary>
    /// 执行关卡完成逻辑
    /// </summary>
    void CompleteLevel()
    {
        _levelCompleted = true;

        Debug.Log("胜利！");

        // 播放胜利音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayWinSfx();
        }

        // 显示胜利 UI
        if (winUiObject != null)
        {
            winUiObject.SetActive(true);
        }

        // 停止玩家移动
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            Rigidbody2D playerRb = playerObj.GetComponent<Rigidbody2D>();
            if (playerRb != null)
            {
                playerRb.velocity = Vector2.zero;
                playerRb.isKinematic = true; 
            }
        }
    }

    private void Update()
    {
        // 胜利后允许按 R 键重置关卡
        if (_levelCompleted && Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }

    void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}