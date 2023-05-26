using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField]
    private Rigidbody2D rbody;


    [SerializeField]
    private Transform groundCheckPoint;

    [SerializeField]
    private LayerMask groundLayerMask;

    [SerializeField]
    private Animator anim;

    private bool isGrounded = false;
    private float groundCheckRadius = 0.1f;


    private float xInput;

    private float moveSpeed = 450f;
    private float jumpForce = 13f;

    private bool canDoubleJump = true;
    private bool facingRight = true;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        xInput = Input.GetAxis("Horizontal");

        isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayerMask) && rbody.velocity.y <= 0.01;
        anim.SetBool("isGrounded", isGrounded);

        if (!facingRight && rbody.velocity.x > 0.01 || facingRight && rbody.velocity.x < -0.01)
        {
            Flip();
        }
        
        if(Input.GetButtonDown("Jump"))
        {
            if (isGrounded)
            {
                Jump();
                canDoubleJump = true;
                anim.SetTrigger("jump");
            }
            else if (canDoubleJump)
            {
                Jump();
                canDoubleJump = false;
                anim.SetTrigger("jump");
            }
        }

        bool isRunning = xInput > 0.01 || xInput < -0.01;
        anim.SetBool("isRunning", isRunning);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(groundCheckPoint.position, groundCheckRadius);
    }

    private void FixedUpdate()
    {
        float xVel = xInput * moveSpeed * Time.deltaTime;
        rbody.velocity = new Vector2(xVel, rbody.velocity.y);
    }

    private void Jump()
    {
        rbody.velocity = new Vector2(rbody.velocity.x, 0f);
        rbody.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
    }

    private void Flip()
    {
        facingRight = !facingRight;
        transform.Rotate(new Vector3(0, 180, 0));
    }
}