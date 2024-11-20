using System;
using Unity.VisualScripting;
using UnityEngine;

public class Player : Character
{
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private float speed = 5;
    [SerializeField] private float jumpForce = 450;
    [SerializeField] private Kunai kunaiPrefab;
    [SerializeField] private Transform throwPoint;
    [SerializeField] private GameObject attackArea;

    private bool isGrounded = true;
    private bool isJumping = false;
    private bool isAttack = false;
    private bool isDeath = false;

    private float horizontal;
    
    private int coin = 0;

    private Vector3 savePoint;


    // Update is called once per frame
    void FixedUpdate()
    {
        if (isDeath)
        {
            return;
        }

        isGrounded = CheckGrounded();
        horizontal = Input.GetAxisRaw("Horizontal");
        //vertical = Input.GetAxisRaw("Vertical");
        
        if (isAttack)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (isGrounded)
        {
            if (isJumping)
            {
                return;
            }
            //jump
            if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            {
                Jump();


            }
            //change anim run
            if (Mathf.Abs(horizontal) > 0.1f)
            {
                ChangeAnim("run");

            }
            //attack
            if (Input.GetKeyDown(KeyCode.C) && isGrounded)
            {
                Attack();
            }
            //throw
            if (Input.GetKeyDown(KeyCode.V) && isGrounded)
            {
                Throw();
            }

        }
        //check falling
        if (!isGrounded && rb.linearVelocity.y < 0)
        {
            ChangeAnim("fall");
            isJumping = false;

        }

        //moving
        if (Mathf.Abs(horizontal) > 0.1f )
        {
            ChangeAnim("run");
            rb.linearVelocity = new Vector2(horizontal * speed, rb.linearVelocity.y);

            transform.rotation = Quaternion.Euler(new Vector3(0, horizontal > 0 ? 0 : 180, 0));
            //transform.localScale = new Vector3(horizontal, 1, 1);
        }
        else if (isGrounded)
        {
            ChangeAnim("idle");
           
            rb.linearVelocity = Vector2.zero;
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

        //if (hit.collider != null)
        //{
        //    return true;
        //}
        //else
        //{
        //    return false;

        //}
        return hit.collider != null;


    }
    private void Attack()
    {
        ChangeAnim("attack");
        isAttack = true;
        Invoke(nameof(ResetAttack), 0.5f);
        ActiveAttack();
        Invoke(nameof(DeActiveAttack), 0.5f);

    }

    private void Throw()
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
    private void Jump()
    {
        isJumping = true;
        ChangeAnim("jump");

        rb.AddForce(jumpForce * Vector2.up);
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Coin")
        {
            coin++;
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
