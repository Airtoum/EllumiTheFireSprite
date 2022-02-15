using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CollisionData))]

public class DynamicCharacter : Character
{
    protected const int INPUT_LEFT = 1 << 0;
    protected const int INPUT_RIGHT = 1 << 1;
    protected const int INPUT_UP = 1 << 2;
    protected const int INPUT_DOWN = 1 << 3;
    protected const int INPUT_JUMP = 1 << 4;
    protected const int INPUT_SPECIAL = 1 << 5;

    private Rigidbody2D rb;
    private CollisionData cd;

    [SerializeField] protected float moveSpeed = 4f;
    
    protected int inputFlags = 0;

    [SerializeField] protected float coyoteTime = 0.08f;
    protected float coyoteTimeTimer = 0;

    [SerializeField] protected Vector2 gravitationalAcceleration = Vector2.down;

    [SerializeField] protected float steepestSlopeDegrees = 45f;
    private bool onGround = false;
    [SerializeField] protected float jumpAmount = 5; 
    
    // Start is called before the first frame update
    protected void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cd = GetComponent<CollisionData>();
    }

    // Update is called once per frame
    protected void FixedUpdate()
    {
        StartCoroutine(DoMovement());
    }

    protected IEnumerator DoMovement()
    {
        Vector2 velocity = rb.velocity;

        float horizontal_movement = 0;
        float vertical_movement = velocity.y;
        horizontal_movement += ((inputFlags & INPUT_RIGHT) > 0) ? 1f : 0f;
        horizontal_movement += ((inputFlags & INPUT_LEFT) > 0) ? -1f : 0f;
        vertical_movement += (onGround && (inputFlags & INPUT_JUMP) > 0) ? jumpAmount : 0f;
        velocity = new Vector2(horizontal_movement * moveSpeed, vertical_movement);
        velocity += gravitationalAcceleration * Time.fixedDeltaTime;
        rb.velocity = velocity;

        yield return new WaitForFixedUpdate();
        onGround = false;
        cd.IterateOverCollisions(EvaluateCollision);
        //GetComponent<SpriteRenderer>().flipX = onGround;
    }

    protected new void EvaluateCollision(Collision2D coll, ContactPoint2D contact)
    {
        if (Mathf.Abs(Vector2.Angle(contact.normal, Vector2.up)) <= steepestSlopeDegrees) {
            onGround = true;                        
        }
    }

}
