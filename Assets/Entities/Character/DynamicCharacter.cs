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

    [SerializeField] private LayerMask AITerrainMask;
    [SerializeField] private float AILookDistance;
    [SerializeField] public bool AIActivated = false;
    [SerializeField] public Vector2 AIDestination;
    [SerializeField] public float AICloseEnoughDistance;
    [SerializeField] public float AICliffDropDistance = 6f;

    protected void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cd = GetComponent<CollisionData>();
        cl = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    protected void FixedUpdate()
    {
        AIInput();
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

    protected RaycastHit2D CreateRaycast(string side, float deviation)
    {
        Vector2 origin = transform.position;
        Vector2 direction = Vector2.right;
        Bounds bounds = cl.bounds;
        switch (side) {
            case "right":
                origin = (Vector2)bounds.center + new Vector2(bounds.extents.x, bounds.extents.y * deviation);
                direction = Vector2.right;
                break;
            case "left":
                origin = (Vector2)bounds.center + new Vector2(-bounds.extents.x, bounds.extents.y * deviation);
                direction = Vector2.left;
                break;
            case "up":
                origin = (Vector2)bounds.center + new Vector2(bounds.extents.x * deviation, bounds.extents.y);
                direction = Vector2.up;
                break;
            case "down":
                origin = (Vector2)bounds.center + new Vector2(bounds.extents.x * deviation, -bounds.extents.y);
                direction = Vector2.down;
                break;
        }
        RaycastHit2D ray_hit = Physics2D.Raycast(origin, direction, AILookDistance, AITerrainMask);
        return ray_hit;
    }

    public void MoveToPoint(Vector2 destination)
    {
        AIActivated = true;
        AIDestination = destination;
    }

    protected void AIInput()
    {
        if (!AIActivated) return;
        if (Vector2.Distance(transform.position, AIDestination) <= AICloseEnoughDistance) {
            AIActivated = false;
            return;
            // perhaps allow for chaining of MoveToPoints in a sequence
        }
        
        /*
        // calculate some useful constants
        float max_jump_height = - jumpAmount * jumpAmount / (2 * gravitationalAcceleration.y);
        
        // this initializes AIRays, a collection of data about the environment.
        // AIRays[0-4] are on the right. AIRays[5-9] are on the top.
        // AIRays[10-14] are on the left. AIRays[15-19] are on the bottom.
        RaycastHit2D[] AIRays = new RaycastHit2D[4*5];
        int i = 0;
        foreach (string direction in new []{"right","up","left","down"}) {
            foreach (float deviation in new []{-1.6f,-0.8f,0.0f,0.8f,1.6f}) {
                AIRays[i] = CreateRaycast(direction, deviation);
                i++;
            }
        }
        */

        ExplorePlatform();

    }

    protected void ExplorePlatform()
    {
        Vector2 pos = transform.position;
        float left = ExploreLateral(pos, false, 0, 20);
        float right = ExploreLateral(pos, true, 0, 20);
        float dist = Vector2.Distance(pos, AIDestination);
        if (left < right) {
            inputFlags = INPUT_LEFT;
        } else {
            inputFlags = INPUT_RIGHT;
        }

        if (Mathf.Min(left, right) > dist - AICloseEnoughDistance) {
            AIActivated = false;
        }
    }

    protected float ExploreDown(Vector2 pos, bool is_right, int depth, int max_depth)
    {
        // not implemented
        return 0;
    }
    
    // returns how close it gets to AIDestination on this path
    protected float ExploreLateral(Vector2 pos, bool is_right, int depth, int max_depth)
    {
        float dist = Vector2.Distance(pos, AIDestination);
        if (depth > max_depth) return dist;
        float dx = (is_right ? 1f : -1f) * cl.bounds.size.x;
        float dy = cl.bounds.size.y;
        RaycastHit2D down = Physics2D.Raycast(pos, Vector2.down, AICliffDropDistance, AITerrainMask);
        if (down) {
            // there is ground beneath us
            RaycastHit2D lateral = Physics2D.Raycast(pos, new Vector2(dx, 0f), Mathf.Abs(dx), AITerrainMask);
            if (lateral) {
                // there's something to our side
                Debug.DrawLine(lateral.point, lateral.point + lateral.normal, Color.yellow);
                if (Mathf.Abs(Vector2.Angle(lateral.normal, Vector2.up)) <= steepestSlopeDegrees) {
                    // we've hit a slope, repeat higher
                    float result = ExploreLateral(pos + new Vector2(dx, cl.bounds.size.y), is_right, depth, max_depth);
                    DebugSquare(pos, Color.yellow);
                    return Mathf.Min(dist, result);
                } else {
                    // we've hit a wall, Explore Wall
                    float result = ExploreWall(pos, is_right, depth, max_depth, 0);
                    DebugSquare(pos, Color.magenta);
                    return Mathf.Min(dist, result);
                }
            } else {
                // keep going
                float result = ExploreLateral(pos + new Vector2(dx, 0f), is_right, depth + 1, max_depth);
                DebugSquare(pos, Color.green);
                return Mathf.Min(dist, result);
            }
        } else {
            // there was no ground found, this is an edge
            DebugSquare(pos, Color.red);
            return dist;
        }
    }

    protected float ExploreWall(Vector2 pos, bool is_right, int depth, int max_depth, int wall_depth)
    {
        float dist = Vector2.Distance(pos, AIDestination);
        if (depth > max_depth) return dist;
        float max_jump_height = - jumpAmount * jumpAmount / (2 * gravitationalAcceleration.y);
        float dy = cl.bounds.size.y;
        float dx = (is_right ? 1f : -1f) * cl.bounds.size.x;
        int max_wall_depth = Mathf.FloorToInt(max_jump_height / dy); 
        if (wall_depth > max_wall_depth) return dist;
            
        RaycastHit2D lateral = Physics2D.Raycast(pos, new Vector2(dx, 0f), dx, AITerrainMask);
        if (lateral) {
            // the wall is still there
            RaycastHit2D up = Physics2D.Raycast(pos, Vector2.up, dy, AITerrainMask);
            if (up) {
                // there is a ceiling
                DebugDiamond(pos, Color.red);
                return dist;
            } else {
                // the wall continues up
                float result = ExploreWall(pos + new Vector2(0f, dy), is_right, depth + 1, max_depth, wall_depth + 1);
                DebugDiamond(pos, Color.magenta);
                return Mathf.Min(dist, result);
            }
        } else {
            // the wall is gone, implies ground
            float result = ExploreLateral(pos + new Vector2(dx, 0f), is_right, depth + 1, max_depth);
            DebugDiamond(pos, Color.green);
            return Mathf.Min(dist, result);
        }
    }
    
    private void DebugSquare(Vector3 pos, Color color)
    {
        Vector3 width = new Vector3(0.1f, 0f, 0f);
        Vector3 height = new Vector3(0f, 0.1f, 0f);
        Vector3 start = pos - width - height;
        Debug.DrawLine(start, start + width * 2, color, 0, false);
        Debug.DrawLine(start, start + height * 2, color, 0, false);
        Debug.DrawLine(start + width * 2, start + width * 2 + height * 2, color, 0, false);
        Debug.DrawLine(start + height * 2, start + width * 2 + height * 2, color, 0, false);
    }

    private void DebugDiamond(Vector3 pos, Color color)
    {
        Vector3 width = new Vector3(0.1f, 0f, 0f);
        Vector3 height = new Vector3(0f, 0.1f, 0f);
        Debug.DrawLine(pos - height, pos - width, color, 0, false);
        Debug.DrawLine(pos - height, pos + width, color, 0, false);
        Debug.DrawLine(pos - width, pos + height, color, 0, false);
        Debug.DrawLine(pos + width, pos + height, color, 0, false);
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
