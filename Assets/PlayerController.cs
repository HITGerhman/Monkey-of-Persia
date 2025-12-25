using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    // ... (之前的变量保持不变) ...
    [Header("核心参数")]
    public float moveSpeed = 10f;
    public float jumpForce = 16f;
    public float fallMultiplier = 2.5f;
    public float lowJumpMultiplier = 2f;

    [Header("手感优化")]
    public float coyoteTime = 0.1f;
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

    // 【新增】死亡状态标记
    public bool IsDead { get; private set; } // 公有属性，供其他脚本读取
    private SpriteRenderer _sr; // 用来变色

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        // 【新增】获取渲染组件以便变色
        _sr = GetComponent<SpriteRenderer>();
        _anim = GetComponent<Animator>();
    }

    private void Update()
    {
        // 【新增】如果死了，直接切断输入，后续代码都不执行
        if (IsDead)
        {
            // 这里可以加一个检测：如果玩家按了倒流键，就允许脚本继续运行（交给 TimeBody 处理）
            // 但为了简单，我们在下面单独处理复活
            return;
        }

        // ... (原有的 ProcessInput, UpdateTimers, CheckJump 逻辑) ...
        // (为了节省篇幅，这里省略原有代码，请保持你原来 Update 里的内容)
        // 建议把之前的逻辑封装成 ProcessInput() 等函数，或者直接保留原样

        // --- 以下是原 Update 代码的精简版引用 ---
        _moveInput = Input.GetAxisRaw("Horizontal");
        // 【新增】处理图片翻转
    if (_moveInput > 0)
        {
         _sr.flipX = false; // 向右跑，不翻转
        }
    else if (_moveInput < 0)
    {
        _sr.flipX = true;  // 向左跑，翻转 X 轴
    }
        _isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, checkRadius, whatIsGround);

        if (_isGrounded) _coyoteTimeCounter = coyoteTime;
        else _coyoteTimeCounter -= Time.deltaTime;

        if (Input.GetButtonDown("Jump")) _jumpBufferCounter = jumpBufferTime;
        else _jumpBufferCounter -= Time.deltaTime;

        if (_jumpBufferCounter > 0f && _coyoteTimeCounter > 0f)
        {
            _rb.velocity = new Vector2(_rb.velocity.x, jumpForce);
            _jumpBufferCounter = 0f;
            _coyoteTimeCounter = 0f;
        }
        //  驱动动画
        // 我们传入绝对值 (Mathf.Abs)，因为不管向左跑(-1)还是向右跑(1)，速度都是正的
        if (_anim != null)
        {
            _anim.SetFloat("Speed", Mathf.Abs(_moveInput));
        }

        if (_rb.velocity.y < 0) _rb.gravityScale = fallMultiplier;
        else if (_rb.velocity.y > 0 && !Input.GetButton("Jump")) _rb.gravityScale = lowJumpMultiplier;
        else _rb.gravityScale = 1f;
    }

    private void FixedUpdate()
    {
        // 【新增】死了就不要再给刚体施加力了
        if (IsDead) return;

        _rb.velocity = new Vector2(_moveInput * moveSpeed, _rb.velocity.y);
    }

    // 【新增】碰撞检测逻辑
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // 只有活着的时候才能死
        if (!IsDead && collision.CompareTag("Danger"))
        {
            Die();
        }
    }

    // 【新增】死亡处理
    void Die()
    {
        IsDead = true;

        // 视觉反馈：变红
        _sr.color = Color.red;

        // 物理反馈：停下并冻结
        _rb.velocity = Vector2.zero;
        _rb.isKinematic = true; // 挂在刺上，不要掉下去

        Debug.Log("你死了！按回车倒流！");
    }

    // 【新增】复活方法（将被 TimeBody 调用）
    public void Resurrect(bool resetColor = true)
    {
        IsDead = false;
        //_sr.color = Color.green; // 恢复原来的颜色（或者白色）
        // 【新增逻辑】
        // 如果 resetColor 是 true，我们强制变绿（比如关卡重置时）
        // 如果 resetColor 是 false（比如时间倒流时），我们就不动颜色，交给 TimeBody 去回溯
        if (resetColor)
        {
            _sr.color = Color.green; 
        }
        _rb.isKinematic = false; // 恢复物理
    }

    // ... (OnDrawGizmos 保持不变) ...
    private void OnDrawGizmosSelected()
    {
        if (groundCheckPoint == null) return;
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(groundCheckPoint.position, checkRadius);
    }
}