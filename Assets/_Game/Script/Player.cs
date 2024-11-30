using System;
using Unity.VisualScripting;
using UnityEngine;

public class Player : Character
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float speed = 5;
    [SerializeField] private float jumpForce = 450;
    [SerializeField] private float glideGravityScale = 0.5f; // Tốc độ trọng lực giảm khi glide
    [SerializeField] private float slideSpeed = 8; // Tốc độ khi trượt
    [SerializeField] private Kunai kunaiPrefab;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private GameObject attackArea;
    [SerializeField] private CapsuleCollider2D normalCollider; // Collider bình thường
    [SerializeField] private CapsuleCollider2D slideCollider; // Collider khi trượt

    private bool isGrounded = true;
    private bool isJumping = false;
    private bool isAttack = false;
    private bool isDeath = false;
    private bool isGliding = false; // Trạng thái glide
    private bool isSliding = false; // Trạng thái slide

    private float horizontal;
    private int coin = 0;
    private Vector3 savePoint;

    private void Awake()
    {
        coin = PlayerPrefs.GetInt("coin", 0);
    }

    void FixedUpdate()
    {
        if (isDeath) return;

        isGrounded = CheckGrounded();
        horizontal = Input.GetAxisRaw("Horizontal");
        //vertical = Input.GetAxisRaw("Vertical");


        if (isAttack)
        {
            rb.velocity = Vector2.zero;
            return;
        }

        if (isGrounded)
        {
            // Nếu nhân vật đang trượt
            
            if (isSliding)
            {
                rb.velocity = new Vector2(horizontal * slideSpeed, rb.velocity.y);
                return;
            }

            if (isJumping)
            {
                return;
            }

            // Nhảy
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Jump();
                return;
            }

            // Bắt đầu trượt
            if (Input.GetKey(KeyCode.LeftShift) && Mathf.Abs(horizontal) > 0.1f)
            {
                Slide();
                ChangeAnim("slide");
                return;
            }

            // Chạy
            if (Mathf.Abs(horizontal) > 0.1f)
            {
                ChangeAnim("run");
            }

            // Tấn công
            if (Input.GetKeyDown(KeyCode.C))
            {
                Attack();
                return;
            }

            // Ném
            if (Input.GetKeyDown(KeyCode.V))
            {
                Throw();
                return;
            }
        }
        else
        {
            // Check nhấn giữ phím để glide
            if (Input.GetKey(KeyCode.Space))
            {
                Glide();
            }
            else
            {
                ResetGlide();
            }

            // Kiểm tra trạng thái rơi
            if (rb.velocity.y < 0)
            {
                ChangeAnim("fall");
                isJumping = false;
            }
        }

        // Di chuyển nhân vật
        if (Mathf.Abs(horizontal) > 0.1f && !isSliding)
        {
            ChangeAnim("run");
            rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
            transform.rotation = Quaternion.Euler(new Vector3(0, horizontal > 0 ? 0 : 180, 0));
        }
        else if (isGrounded && !isSliding)
        {
            ChangeAnim("idle");
            rb.velocity = Vector2.zero;
        }
    }

    public override void OnInit()
    {
        base.OnInit();
        isDeath = false;
        isAttack = false;
        transform.position = savePoint;
        ChangeAnim("idle");
        DeActiveAttack();
        SavePoint();
        UIManager.instance.SetCoin(coin);
    }

    public override void OnDespawn()
    {
        base.OnDespawn();
        OnInit();
    }

    protected override void OnDeath()
    {
        base.OnDeath();
    }

    private bool CheckGrounded()
    {
        Debug.DrawLine(transform.position, transform.position + Vector3.down * 1.1f, Color.red);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1.1f, groundLayer);
        return hit.collider != null;
    }

    public void Attack()
    {
        ChangeAnim("attack");
        isAttack = true;
        Invoke(nameof(ResetAttack), 0.5f);
        ActiveAttack();
        Invoke(nameof(DeActiveAttack), 0.5f);
    }

    public void Throw()
    {
        ChangeAnim("throw");
        isAttack = true;
        Invoke(nameof(ResetAttack), 0.5f);
        Instantiate(kunaiPrefab, throwPoint.position, throwPoint.rotation);
    }

    private void ResetAttack()
    {
        ChangeAnim("idle");
        isAttack = false;
    }

    public void Jump()
    {
        isJumping = true;
        ChangeAnim("jump");
        rb.AddForce(jumpForce * Vector2.up);
    }

    public void Glide()
    {
        if (!isGrounded && rb.velocity.y < 0)
        {
            if (!isGliding)
            {
                isGliding = true;
                ChangeAnim("glide");
                rb.gravityScale = glideGravityScale; // Giảm trọng lực khi glide
            }
        }
    }

    public void ResetGlide()
    {
        if (isGliding)
        {
            isGliding = false;
            rb.gravityScale = 1; // Trở lại trọng lực bình thường
        }
    }

    public void Slide()
    {
        if (isGrounded && !isSliding)
        {
            isSliding = true;
            ChangeAnim("slide");

            // Chuyển sang collider trượt
            normalCollider.enabled = false;
            slideCollider.enabled = true;

            Invoke(nameof(ResetSlide), 0.5f); // Kết thúc trượt sau 0.5 giây
        }
    }

    private void ResetSlide()
    {
        isSliding = false;
        normalCollider.enabled = true;
        slideCollider.enabled = false;
        ChangeAnim("idle");
    }

    internal void SavePoint()
    {
        savePoint = transform.position;
    }

    private void ActiveAttack()
    {
        attackArea.SetActive(true);
    }

    private void DeActiveAttack()
    {
        attackArea.SetActive(false);
    }

    public void SetMove(float horizontal)
    {
        this.horizontal = horizontal;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Coin")
        {
            coin++;
            PlayerPrefs.SetInt("coin", coin);
            UIManager.instance.SetCoin(coin);
            Destroy(collision.gameObject);
        }
        if (collision.tag == "DeathZone")
        {
            isDeath = true;
            ChangeAnim("die");
            Invoke(nameof(OnInit), 1f);
        }
    }
}