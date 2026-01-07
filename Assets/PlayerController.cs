using UnityEngine;

/// <summary>
/// 玩家控制器。
/// 负责处理玩家的移动、跳跃、物理检测以及死亡/复活状态。
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Header("核心参数")]
    [Tooltip("玩家移动速度")]
    public float moveSpeed = 10f;
    [Tooltip("跳跃力度")]
    public float jumpForce = 16f;
    [Tooltip("下落时的重力倍率（优化跳跃手感）")]
    public float fallMultiplier = 2.5f;
    [Tooltip("小跳时的重力倍率（按住跳跃键时间越短跳得越低）")]
    public float lowJumpMultiplier = 2f;

    [Header("手感优化")]
    [Tooltip("土狼时间：离开地面后的一小段时间内仍可跳跃")]
    public float coyoteTime = 0.1f;
    [Tooltip("跳跃缓冲：在落地前按下跳跃键，落地瞬间会自动跳跃")]
    public float jumpBufferTime = 0.1f;

    [Header("检测设置")]
    public Transform groundCheckPoint;
    public float checkRadius = 0.2f;
    public LayerMask whatIsGround;

    private Rigidbody2D _rb;
    private float _moveInput;
    private bool _isGrounded;
    private float _coyoteTimeCounter;
    private float _jumpBufferCounter;
    private Animator _anim;

    /// <summary>
    /// 获取当前是否处于死亡状态
    /// </summary>
    public bool IsDead { get; private set; }
    
    private SpriteRenderer _sr;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _sr = GetComponent<SpriteRenderer>();
        _anim = GetComponent<Animator>();
    }

    private void Update()
    {
        // 死亡状态下禁止输入控制
        if (IsDead)
        {
            return;
        }

        ProcessInput();
        UpdateTimers();
        CheckJump();
        UpdateAnimation();
        ApplyGravityModifiers();
    }

    private void ProcessInput()
    {
        _moveInput = Input.GetAxisRaw("Horizontal");

        // 根据移动方向翻转 Sprite
        if (_moveInput > 0)
        {
            _sr.flipX = false;
        }
        else if (_moveInput < 0)
        {
            _sr.flipX = true;
        }
    }

    private void UpdateTimers()
    {
        _isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, checkRadius, whatIsGround);

        // 更新土狼时间计时器
        if (_isGrounded) _coyoteTimeCounter = coyoteTime;
        else _coyoteTimeCounter -= Time.deltaTime;

        // 更新跳跃缓冲计时器
        if (Input.GetButtonDown("Jump")) _jumpBufferCounter = jumpBufferTime;
        else _jumpBufferCounter -= Time.deltaTime;
    }

    private void CheckJump()
    {
        // 当跳跃缓冲有效且处于土狼时间内（即视为在地面）时触发跳跃
        if (_jumpBufferCounter > 0f && _coyoteTimeCounter > 0f)
        {
            _rb.velocity = new Vector2(_rb.velocity.x, jumpForce);
            _jumpBufferCounter = 0f;
            _coyoteTimeCounter = 0f;

            // 播放跳跃音效
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlayJumpSfx();
            }
        }
    }

    private void UpdateAnimation()
    {
        if (_anim != null)
        {
            // 设置速度参数（使用绝对值因为 Animator通常只关心速度大小）
            _anim.SetFloat("Speed", Mathf.Abs(_moveInput));
        }
    }

    private void ApplyGravityModifiers()
    {
        // 下落时增加重力，使跳跃手感更利落
        if (_rb.velocity.y < 0) 
            _rb.gravityScale = fallMultiplier;
        // 小跳逻辑：上升期间松开跳跃键，增加重力快速下落
        else if (_rb.velocity.y > 0 && !Input.GetButton("Jump")) 
            _rb.gravityScale = lowJumpMultiplier;
        else 
            _rb.gravityScale = 1f;
    }

    private void FixedUpdate()
    {
        if (IsDead) return;

        // 应用水平移动速度
        _rb.velocity = new Vector2(_moveInput * moveSpeed, _rb.velocity.y);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!IsDead && collision.CompareTag("Danger"))
        {
            Die();
        }
    }

    /// <summary>
    /// 处理角色死亡逻辑
    /// </summary>
    void Die()
    {
        if (IsDead) return;
        IsDead = true;
        
        if (_anim != null) 
        {
            _anim.SetBool("IsDead", true);
        }

        // 停止物理运动并设为运动学，防止尸体滑落
        _rb.velocity = Vector2.zero;
        _rb.isKinematic = true;

        // 播放死亡音效
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.PlayDieSfx();
        }

        Debug.Log("你死了！按回车倒流！");
    }

    /// <summary>
    /// 复活角色（通常由 TimeBody 调用）
    /// </summary>
    /// <param name="resetColor">是否重置颜色（时间倒流时不应重置颜色，而是由倒流逻辑还原）</param>
    public void Resurrect(bool resetColor = true)
    {
        IsDead = false;
        if (_anim != null)
        {
            _anim.SetBool("IsDead", false);
        }

        // 恢复物理模拟
        _rb.isKinematic = false; 
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheckPoint.position, checkRadius);
    }
}