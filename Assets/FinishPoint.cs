using UnityEngine;
using UnityEngine.SceneManagement; // 需要这个来重置场景

public class FinishPoint : MonoBehaviour
{
    [Header("UI 设置")]
    public GameObject winUiObject; // 拖入刚才做的 WinText

    private bool _levelCompleted = false;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 如果碰到的是主角，且关卡还没结束
        if (!_levelCompleted && collision.gameObject.CompareTag("Player"))
        {
            CompleteLevel();
        }
    }

    void CompleteLevel()
    {
        _levelCompleted = true;

        Debug.Log("胜利！");

        // 1. 显示胜利 UI
        if (winUiObject != null)
        {
            winUiObject.SetActive(true);
        }

        // 2. (可选) 播放胜利音效
        // 3. (可选) 冻结主角，防止他跑出屏幕，或者让他停在旗帜那欢呼
        // 这里我们要获取主角的 Rigidbody 来让他停下
        Rigidbody2D playerRb = GameObject.FindGameObjectWithTag("Player").GetComponent<Rigidbody2D>();
        if (playerRb != null)
        {
            playerRb.velocity = Vector2.zero;
            playerRb.isKinematic = true; // 类似于死亡时的冻结，但这次是开心的冻结
        }
    }

    private void Update()
    {
        // 只有在胜利后，按 R 才能重置
        if (_levelCompleted && Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }

    void RestartGame()
    {
        // 获取当前场景的名字，并重新加载它
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}