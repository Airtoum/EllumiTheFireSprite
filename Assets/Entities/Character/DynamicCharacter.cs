using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]

public class DynamicCharacter : Character
{
    protected const int INPUT_LEFT = 1 << 0;
    protected const int INPUT_RIGHT = 1 << 1;
    protected const int INPUT_UP = 1 << 2;
    protected const int INPUT_DOWN = 1 << 3;
    protected const int INPUT_JUMP = 1 << 4;
    protected const int INPUT_SPECIAL = 1 << 5;

    private Rigidbody2D rb;

    [SerializeField] protected float moveSpeed = 4f;
    
    protected int inputFlags = 0;
    
    // Start is called before the first frame update
    protected void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    protected void FixedUpdate()
    {
        DoMovement();
    }
    
    protected void DoMovement()
    {
        float horizontal_movement = 0;
        horizontal_movement += ((inputFlags & INPUT_RIGHT) > 0) ? 1f : 0f;
        horizontal_movement += ((inputFlags & INPUT_LEFT) > 0) ? -1f : 0f;
        rb.velocity = new Vector2(horizontal_movement * moveSpeed, 0f);
    }
    
}
