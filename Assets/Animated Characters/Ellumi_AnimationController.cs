using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ellumi_AnimationController : MonoBehaviour
{
    [SerializeField] Animator componentAnimator;
    [SerializeField] Rigidbody2D mainRigidBody;

    [SerializeField] FireSprite ellumiScript;
    [SerializeField] GameObject currentJumpParticle;
    [SerializeField] GameObject jumpParticleRight;
    [SerializeField] GameObject jumpParticleLeft;
    [SerializeField] GameObject jumpParticleSpawnPoint;

    [SerializeField] float minimumJumpHeight;

    Ray2D leftGroundCheck;
    Ray2D midGroundCheck;
    Ray2D rightGroundCheck;

    private float xScale;
    private Vector3 fullScale;

    private Vector3 jumpFullScale;

    private bool jumpStarted = false;

    private bool facingLeft = false;

    private void Awake() 
    {
        componentAnimator = this.gameObject.GetComponent<Animator>();
        mainRigidBody = this.transform.parent.gameObject.GetComponent<Rigidbody2D>();
        ellumiScript = this.transform.parent.gameObject.GetComponent<FireSprite>();
        fullScale = this.transform.parent.transform.localScale;
        xScale = fullScale.x; 
        currentJumpParticle = jumpParticleRight;

        updateRayCasts();
    }

    private void Start() 
    {
        componentAnimator.SetBool("IsJumping", false);
        componentAnimator.SetFloat("Speed", 0);
    }

    void Update()
    {
        UpdateAnimationState();
        updateRayCasts();
        FlipCheck();
        
        if(Input.GetAxis("Jump") > 0)
        {
            jumpParticleSpawn();
        }
    }

    void UpdateAnimationState()
    {
        float horizontalVelocity = Mathf.Abs(mainRigidBody.velocity.x);
        float verticalVelocity = mainRigidBody.velocity.y;

        componentAnimator.SetFloat("Speed", horizontalVelocity);
        componentAnimator.SetFloat("VertVelocity", verticalVelocity);

        if(checkGrounded())
        {
            componentAnimator.SetBool("IsJumping", false);
            jumpStarted = false;
        }
        else
        {
            componentAnimator.SetBool("IsJumping", true);
        }
    }

    void jumpParticleSpawn()
    {
        if (!checkGrounded() && jumpStarted == false)
        {
            Instantiate(currentJumpParticle, jumpParticleSpawnPoint.transform.position, Quaternion.identity);
            jumpStarted = true;
        }
    }

    void FlipCheck()
    {
        if (Input.GetAxis("Horizontal") < 0 && facingLeft == false)
        {
            this.transform.parent.transform.localScale = new Vector3(-1 * xScale, this.transform.parent.transform.localScale.y, this.transform.parent.transform.localScale.z);
            facingLeft = true;
            currentJumpParticle = jumpParticleLeft;
        }
        else if (Input.GetAxis("Horizontal") > 0 && facingLeft == true)
        {
            this.transform.parent.transform.localScale = fullScale;
            facingLeft = false;
            currentJumpParticle = jumpParticleRight;
        }
    }

    void updateRayCasts()
    {
        leftGroundCheck = new Ray2D(new Vector2((transform.position.x - .5f), transform.position.y), new Vector2(0f, -1));
        midGroundCheck = new Ray2D(new Vector2(transform.position.x, transform.position.y), new Vector2(0f, -1));
        rightGroundCheck = new Ray2D(new Vector2((transform.position.x + .5f), transform.position.y), new Vector2(0f, -1));

        Debug.DrawRay(leftGroundCheck.origin, leftGroundCheck.direction, Color.green, Time.deltaTime);
        Debug.DrawRay(midGroundCheck.origin, midGroundCheck.direction, Color.green, Time.deltaTime);
        Debug.DrawRay(rightGroundCheck.origin, rightGroundCheck.direction, Color.green, Time.deltaTime);
    }

    bool checkGrounded()
    {
        RaycastHit2D leftHit = Physics2D.Raycast(leftGroundCheck.origin, leftGroundCheck.direction);
        RaycastHit2D midHit = Physics2D.Raycast(midGroundCheck.origin, midGroundCheck.direction);
        RaycastHit2D rightHit = Physics2D.Raycast(rightGroundCheck.origin, rightGroundCheck.direction); 

        bool leftClose = leftHit.distance < minimumJumpHeight;
        bool midClose = midHit.distance < minimumJumpHeight;
        bool rightClose = rightHit.distance < minimumJumpHeight;

        if ( (leftClose && midClose) || (midClose && rightClose) || (leftClose && rightClose) )
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
