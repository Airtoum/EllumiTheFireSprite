using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirSpriteAnimationController : MonoBehaviour
{
    [SerializeField] Animator airSpriteAnimator;
    [SerializeField] AirSprite airSprite;
    Rigidbody2D rb;
    private float xScale;
    private Vector3 fullScale;


    void Start()
    {
        rb = this.GetComponent<Rigidbody2D>();
        airSpriteAnimator = this.GetComponentInChildren<Animator>();
        fullScale = this.transform.localScale;
        xScale = fullScale.x;
    }

    void Update()
    {
        animValueUpdate();
        FlipCheck();
    }

    void FlipCheck()
    {
        if (airSprite.facingLeft)
        {
            this.transform.localScale = new Vector3(-1 * xScale, this.transform.localScale.y, this.transform.localScale.z);
        }
        else
        {
            this.transform.localScale = fullScale;
        }
    }

    void animValueUpdate()
    {
        if (Mathf.Abs(rb.velocity.x) > 0.7)
        {
            airSpriteAnimator.SetFloat("Speed",Mathf.Abs(rb.velocity.x));
        }
        else if (Mathf.Abs(rb.velocity.y) > 0.7)
        {
            airSpriteAnimator.SetFloat("Speed",Mathf.Abs(rb.velocity.y));
        }
        else
        {
            airSpriteAnimator.SetFloat("Speed",0);
        }
    }
}
