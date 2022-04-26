using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ellumi_AnimationController : MonoBehaviour
{
    [SerializeField] Animator componentAnimator;
    [SerializeField] Rigidbody2D mainRigidBody;

    [SerializeField] FireSprite ellumiScript;
    [SerializeField] GameObject jumpParticle;
    [SerializeField] GameObject jumpParticleSpawnPoint;
    [SerializeField] AnimationClip jumpParticleAnim;

    private float xScale;
    private Vector3 fullScale;

    private Vector3 jumpFullScale;

    private bool jumpStarted = false;

    private void Awake() 
    {
        componentAnimator = this.gameObject.GetComponent<Animator>();
        mainRigidBody = this.transform.parent.gameObject.GetComponent<Rigidbody2D>();
        ellumiScript = this.transform.parent.gameObject.GetComponent<FireSprite>();
        fullScale = this.transform.parent.transform.localScale;
        xScale = fullScale.x; 
        jumpFullScale = jumpParticle.transform.localScale;
    }

    private void Start() 
    {
        componentAnimator.SetBool("IsJumping", false);
        componentAnimator.SetFloat("Speed", 0);
    }

    void Update()
    {
        UpdateAnimationState();
        FlipCheck();
        jumpParticleSpawn();
    }

    void UpdateAnimationState()
    {
        float horizontalVelocity = Mathf.Abs(mainRigidBody.velocity.x);
        float verticalVelocity = mainRigidBody.velocity.y;

        componentAnimator.SetFloat("Speed", horizontalVelocity);
        componentAnimator.SetFloat("VertVelocity", verticalVelocity);

        if(ellumiScript.GetOnGround())
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
        if (jumpStarted == false && componentAnimator.GetBool("IsJumping"))
        {
            Instantiate(jumpParticle, jumpParticleSpawnPoint.transform.position, Quaternion.identity);
            jumpStarted = true;
        }
    }

    void FlipCheck()
    {
        if (ellumiScript.facingLeft)
        {
            this.transform.parent.transform.localScale = new Vector3(-1 * xScale, this.transform.parent.transform.localScale.y, this.transform.parent.transform.localScale.z);
            jumpParticle.transform.localScale = new Vector3(-1 * jumpParticle.transform.localScale.x, jumpParticle.transform.localScale.y, jumpParticle.transform.localScale.z);
        }
        else
        {
            this.transform.parent.transform.localScale = fullScale;
            jumpParticle.transform.localScale = jumpFullScale;
        }
    }
}
