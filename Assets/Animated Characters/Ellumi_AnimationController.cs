using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ellumi_AnimationController : MonoBehaviour
{
    [SerializeField] Animator componentAnimator;
    [SerializeField] Rigidbody2D mainRigidBody;

    [SerializeField] FireSprite ellumiScript;

    private float xScale;
    private Vector3 fullScale;

    private void Awake() 
    {
        componentAnimator = this.gameObject.GetComponent<Animator>();
        mainRigidBody = this.transform.parent.gameObject.GetComponent<Rigidbody2D>();
        ellumiScript = this.transform.parent.gameObject.GetComponent<FireSprite>();
        fullScale = this.transform.parent.transform.localScale;
        xScale = fullScale.x;
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
        }
        else
        {
            componentAnimator.SetBool("IsJumping", true);
        }
    }

    void FlipCheck()
    {
        if (ellumiScript.facingLeft)
        {
            this.transform.parent.transform.localScale = new Vector3(-1 * xScale, this.transform.parent.transform.localScale.y, this.transform.parent.transform.localScale.z);
        }
        else
        {
            this.transform.parent.transform.localScale = fullScale;
        }
    }
}
