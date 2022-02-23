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
    private Collider2D cl;

    [SerializeField] protected float moveSpeed = 4f;
    [SerializeField] protected float tightness = 0.1f;
    
    protected int inputFlags = 0;

    [SerializeField] protected float coyoteTime = 0.08f;
    protected float coyoteTimeTimer = 0;

    [SerializeField] protected Vector2 gravitationalAcceleration = Vector2.down;

    [SerializeField] protected float steepestSlopeDegrees = 45f;
    protected bool onGround = false;
    [SerializeField] protected float jumpAmount = 5;

    [SerializeField] protected PhysicsMaterial2D staticMaterial;
    [SerializeField] protected PhysicsMaterial2D dynamicMaterial;

    protected Vector2 lastPlatformVelocity = Vector2.zero;

    // I hate using this as a soluton
    protected float jumpCooldown = 0.02f;
    protected float jumpCooldownTimer = 0;
    
    protected void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cd = GetComponent<CollisionData>();
        cl = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    protected void FixedUpdate()
    {
        StartCoroutine(DoMovement());
    }

    protected IEnumerator DoMovement()
    {
        Vector2 velocity = rb.velocity;

        float horizontal_movement = velocity.x;
        float vertical_movement = velocity.y;
        float target_horizontal_movement = 0;
        target_horizontal_movement += ((inputFlags & INPUT_RIGHT) > 0) ? 1f : 0f;
        target_horizontal_movement += ((inputFlags & INPUT_LEFT) > 0) ? -1f : 0f;
        target_horizontal_movement *= moveSpeed;
        cl.sharedMaterial = (target_horizontal_movement == 0f) ? staticMaterial : dynamicMaterial;
        if (onGround && jumpCooldownTimer > jumpCooldown) {
            coyoteTimeTimer = 0;
        }
        if ((onGround || coyoteTimeTimer <= coyoteTime) && (inputFlags & INPUT_JUMP) > 0 && jumpCooldownTimer > jumpCooldown) {
            //vertical_movement += jumpAmount;
            vertical_movement = jumpAmount + lastPlatformVelocity.y;
            //if (!onGround) print("coyote jump! " + coyoteTimeTimer + "s");
            // make sure they don't have coyote time
            coyoteTimeTimer = coyoteTime + 1;
            onGround = false;
            jumpCooldownTimer = 0;
        }

        if (!onGround && Mathf.Abs(horizontal_movement) > Mathf.Abs(target_horizontal_movement) &&
             ((horizontal_movement > 0 && target_horizontal_movement > 0) ||
              (horizontal_movement < 0 && target_horizontal_movement < 0) ||
              (target_horizontal_movement == 0)                              ) ) {
            // if the player is going fast, let them keep going fast
        } else {
            horizontal_movement = horizontal_movement * (1 - tightness) +
                                  target_horizontal_movement * tightness;
        }
        velocity = new Vector2(horizontal_movement, vertical_movement);
        velocity += gravitationalAcceleration * Time.fixedDeltaTime;
        rb.velocity = velocity;

        coyoteTimeTimer += Time.fixedDeltaTime;
        jumpCooldownTimer += Time.fixedDeltaTime;

        yield return new WaitForFixedUpdate();
        onGround = false;
        cd.IterateOverCollisions(EvaluateCollision);
        //GetComponent<SpriteRenderer>().flipX = onGround;
    }

    protected new void EvaluateCollision(Collision2D coll, ContactPoint2D contact)
    {
        if (Mathf.Abs(Vector2.Angle(contact.normal, Vector2.up)) <= steepestSlopeDegrees) {
            onGround = true;
            if (contact.rigidbody) {
                lastPlatformVelocity = contact.rigidbody.velocity;
            } else {
                lastPlatformVelocity = Vector2.zero;
            }
        }
    }

    protected void RaycastWheel()
    {
        Vector2 origin = transform.position;
        for (int i = 0; i < 60; i++) {
            // map [0,60) to [0,2pi) 
            float t = 2f * Mathf.PI * i / 60f;
            Vector2 direction = new Vector2(Mathf.Cos(t), Mathf.Sin(t));
            Physics2D.Raycast(origin, direction);
        }
    }

    protected void AddVelocity(Vector2 vel)
    {
        rb.velocity += vel;
    }

    public virtual void DoAbilityPrimaryDown(Vector3 position)
    {
        // override with cool stuff
    }
    
    public virtual void DoAbilityPrimaryHold(Vector3 position)
    {
        // override with cool stuff
    }

}
