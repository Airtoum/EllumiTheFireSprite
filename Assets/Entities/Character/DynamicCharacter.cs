using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    protected enum AIMoves
    {
        Stop, Lateral, Up, Down, LateralUp, JumpSideways, JumpUpwards, JumpBackwards, Left, Right, JumpLeft, JumpRight
    }

    protected enum AISearchStates
    {
        Lateral, Wall, Jump
    }

    protected Rigidbody2D rb;
    protected CollisionData cd;
    protected Collider2D cl;

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

    [SerializeField] protected LayerMask AITerrainMask;
    [SerializeField] private float AILookDistance;
    [SerializeField] public bool AIActivated = false;
    [SerializeField] public Vector2 AIDestination;
    [SerializeField] public float AICloseEnoughDistance;
    [SerializeField] public float AICliffDropDistance = 6f;
    [SerializeField] public float AICliffDownwarpDistance = 0.5f;
    [SerializeField] public int AIMaxDepth;
    private int debug_AINodeCount = 0;
    private int debug_AILandFromJumpCount = 0;
    private int debug_AIWalksCanceled = 0;
    private int debug_WalkNodes = 0;
    private int debug_WallNodes = 0;
    private int debug_JumpNodes = 0;
    [SerializeField] public float AIJumpTimestep = 0.2f;
    [SerializeField] public float AIJumpPenalization = 0.4f;
    private Dictionary<(Vector2, AISearchStates), int> AIVisited = new Dictionary<(Vector2, AISearchStates), int>();
    [SerializeField] public float AIGridFineness = 0.4f;
    [SerializeField] public float AIDepthPenalty = 0.2f;
    private float AItimeTillRecalculatePath = 0f;
    private List<(Vector2, AIMoves)> AIcurrentPath = new List<(Vector2, AIMoves)>();
    private AIMoves AICurrentMove = AIMoves.Stop;
    [SerializeField] public float AIPathFollowNodeDistance = 0.4f;
    [SerializeField] public int AIPhysicsSteps = 25; 

    [SerializeField] public bool facingLeft = false;

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
        StartCoroutine(DoCharacterPhysics());
        
    }

    protected IEnumerator DoCharacterPhysics()
    {
        DoMovement();
        SetFacing();
        yield return new WaitForFixedUpdate();
        onGround = false;
        cd.IterateOverCollisions(EvaluateCollision);
    }

    protected virtual void DoMovement()
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
            print("jump");
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
    }

    public void SetFacing()
    {
        bool press_left = (inputFlags & INPUT_LEFT) > 0;
        bool press_right = (inputFlags & INPUT_RIGHT) > 0;
        if (press_left && press_right) {
        } else if (press_left) {
            facingLeft = true;
        } else if (press_right) {
            facingLeft = false;
        }
    }

    protected new void EvaluateCollision(Collision2D coll, ContactPoint2D contact)
    {
        if (Mathf.Abs(Vector2.Angle(contact.normal, Vector2.up)) <= steepestSlopeDegrees) {
            // check to make sure it's near his feet
            if (Mathf.Abs(transform.position.x - contact.point.x) < 0.9f * cl.bounds.size.x &&
                 contact.point.y < cl.bounds.center.y - 0.29f * cl.bounds.size.y) {
                onGround = true;
                if (contact.rigidbody) {
                    lastPlatformVelocity = contact.rigidbody.velocity;
                } else {
                    lastPlatformVelocity = Vector2.zero;
                }
            }
        }
    }
    

    public void MoveToPoint(Vector2 destination)
    {
        AIActivated = true;
        AIDestination = destination;
    }

        protected virtual void AIInput()
    {
        if (!AIActivated) return;
        if (Vector2.Distance(transform.position, AIDestination) <= AICloseEnoughDistance) {
            AIActivated = false;
            inputFlags = 0;
            return;
            // perhaps allow for chaining of MoveToPoints in a sequence
        }

        if (onGround) {
            ExplorePlatform();
        } else {
            // leave current inputflags untouched
        }

    }

    protected void ExplorePlatform()
    {
        Vector2 pos = transform.position;
        (float, AIMoves) left = ExploreLateral(pos, false, 0, 20);
        (float, AIMoves) right = ExploreLateral(pos, true, 0, 20);
        DebugText.SetText(Mathf.Min(left.Item1, right.Item1).ToString());
        float dist = Vector2.Distance(pos, AIDestination);
        int lateral_choice = 0;
        AIMoves selected_input;
        if (left.Item1 < right.Item1) {
            lateral_choice = INPUT_LEFT;
            selected_input = left.Item2;
        } else {
            lateral_choice = INPUT_RIGHT;
            selected_input = right.Item2;
        }

        switch (selected_input) {
            case AIMoves.Lateral:
                inputFlags = lateral_choice;
                break;
            case AIMoves.LateralUp:
                inputFlags = INPUT_JUMP | lateral_choice;
                break;
            case AIMoves.Up:
                inputFlags = INPUT_JUMP;
                break;
            case AIMoves.Stop:
                inputFlags = 0;
                break;
            case AIMoves.Down:
                inputFlags = 0;
                break;
            case AIMoves.JumpLeft:
                inputFlags = INPUT_JUMP | INPUT_LEFT;
                break;
            case AIMoves.JumpUpwards:
                inputFlags = INPUT_JUMP;
                break;
            case AIMoves.JumpRight:
                inputFlags = INPUT_JUMP | INPUT_RIGHT;
                break;
            default:
                inputFlags = 0;
                break;
        }

        if (Mathf.Min(left.Item1, right.Item1) > dist - AICloseEnoughDistance) {
            AIActivated = false;
            inputFlags = 0;
        }
    }

    protected float ExploreDown(Vector2 pos, bool is_right, int depth, int max_depth)
    {
        // not implemented
        return 0;
    }
    
    // returns how close it gets to AIDestination on this path
    protected (float, AIMoves) ExploreLateral(Vector2 pos, bool is_right, int depth, int max_depth)
    {
        float dist = Vector2.Distance(pos, AIDestination);
        if (depth > max_depth) return (dist, AIMoves.Stop);
        float dx = (is_right ? 1f : -1f) * cl.bounds.size.x;
        float dy = cl.bounds.size.y;
        float best = Mathf.Min(dist);
        AIMoves move = AIMoves.Stop;

        void CheckIfBetter(float score, AIMoves new_move)
        {
            if (score < best) {
                best = score + AIDepthPenalty * depth;
                move = new_move;
            }
        }
        
        RaycastHit2D down = Physics2D.Raycast(pos, Vector2.down, AICliffDropDistance, AITerrainMask);
        RaycastHit2D down_left = Physics2D.Raycast(pos + new Vector2(-cl.bounds.extents.x, 0), Vector2.down, AICliffDropDistance, AITerrainMask);
        RaycastHit2D down_right = Physics2D.Raycast(pos + new Vector2(cl.bounds.extents.x, 0), Vector2.down, AICliffDropDistance, AITerrainMask);
        if (down || down_left || down_right) {
            // there is ground beneath us
            float highest_floor = down.distance;
            if (down_left) highest_floor = Mathf.Min(highest_floor, down_left.distance);
            if (down_right) highest_floor = Mathf.Min(highest_floor, down_right.distance);
            pos += Vector2.down * (highest_floor - cl.bounds.extents.y);
            RaycastHit2D lateral = Physics2D.Raycast(pos, new Vector2(dx, 0f), Mathf.Abs(dx), AITerrainMask);
            if (lateral) {
                // there's something to our side
                Debug.DrawLine(lateral.point, lateral.point + lateral.normal, Color.yellow);
                if (Mathf.Abs(Vector2.Angle(lateral.normal, Vector2.up)) <= steepestSlopeDegrees) {
                    // we've hit a slope, repeat higher
                    (float, AIMoves) result = ExploreLateral(pos + new Vector2(dx, cl.bounds.size.y), is_right, depth, max_depth);
                    DebugSquare(pos, Color.yellow);
                    best = Mathf.Min(best, result.Item1);
                    return (best, result.Item2);
                } else {
                    // we've hit a wall, Explore Wall
                    (float, AIMoves) result = ExploreWall(pos, is_right, depth, max_depth, 0);
                    DebugSquare(pos, Color.magenta);
                    best = Mathf.Min(best, result.Item1);
                    return (best, AIMoves.LateralUp);
                }
            } else {
                // keep going
                (float, AIMoves) result = ExploreLateral(pos + new Vector2(dx, 0f), is_right, depth + 1, max_depth);
                DebugSquare(pos, Color.green);
                CheckIfBetter(result.Item1, AIMoves.Lateral);
                for (int i = 0; i < AIMarkers.Markers.Count; i++) {
                    AIMarker marker = AIMarkers.Markers[i];
                    (bool is_marker, AIMarker.MarkerTypes marker_type, Vector2 marker_direction) = marker.ModifyAI(pos);
                    if (is_marker) {
                        switch (marker_type) {
                            case AIMarker.MarkerTypes.JumpMarker:
                                (float, AIMoves) jump_result = ExploreJump(pos, marker_direction, depth + 1, max_depth);
                                AIMoves jump_move = AIMoves.JumpUpwards;
                                if (marker_direction.x < 0) jump_move = AIMoves.JumpLeft;
                                if (marker_direction.x > 0) jump_move = AIMoves.JumpRight;
                                CheckIfBetter(jump_result.Item1, jump_move);
                                break;
                            default:
                                break;
                        }
                    }
                }
                return (best, move);
            }
        } else {
            // there was no ground found, this is an edge
            DebugSquare(pos, Color.red);
            return (best, AIMoves.Stop);
        }
    }

    protected (float, AIMoves) ExploreWall(Vector2 pos, bool is_right, int depth, int max_depth, int wall_depth)
    {
        float dist = Vector2.Distance(pos, AIDestination);
        if (depth > max_depth) return (dist, AIMoves.Stop);
        float max_jump_height = - jumpAmount * jumpAmount / (2 * gravitationalAcceleration.y);
        float dy = cl.bounds.size.y;
        float dx = (is_right ? 1f : -1f) * cl.bounds.size.x;
        int max_wall_depth = Mathf.RoundToInt(max_jump_height / dy); 
        if (wall_depth > max_wall_depth) return (dist, AIMoves.Stop);
        
        float best = Mathf.Min(dist);
        RaycastHit2D lateral = Physics2D.Raycast(pos, new Vector2(dx, 0f), Mathf.Abs(dx), AITerrainMask);
        if (lateral) {
            // the wall is still there
            RaycastHit2D up = Physics2D.Raycast(pos, Vector2.up, dy, AITerrainMask);
            if (up) {
                // there is a ceiling
                DebugDiamond(pos, Color.red);
                return (dist, AIMoves.Stop);
            } else {
                // the wall continues up
                (float, AIMoves) result = ExploreWall(pos + new Vector2(0f, dy), is_right, depth + 1, max_depth, wall_depth + 1);
                DebugDiamond(pos, Color.magenta);
                best = Mathf.Min(best, result.Item1);
                return (best, AIMoves.LateralUp);
            }
        } else {
            // the wall is gone, implies ground
            (float, AIMoves) result = ExploreLateral(pos + new Vector2(dx, 0f), is_right, depth + 1, max_depth);
            DebugDiamond(pos, Color.green);
            best = Mathf.Min(best, result.Item1);
            return (best, AIMoves.Lateral);
        }
    }

    protected (float, AIMoves) ExploreJump(Vector2 pos, Vector2 vel, int depth, int max_depth)
    {
        float best = Vector2.Distance(pos, AIDestination);
        AIMoves move = AIMoves.Stop;
        DebugSquare(pos, Color.red);
        
        float timestep = 0.2f;
        Vector2 next_pos = pos +
                           Ultramath.TrajectoryPos(vel.y, gravitationalAcceleration.y, timestep) * Vector2.up +
                           Vector2.right * vel.x * timestep;
        Vector2 next_vel = vel;
        next_vel.y = Ultramath.TrajectoryVel(vel.y, gravitationalAcceleration.y, timestep);

        void CheckIfBetter(float score, AIMoves new_move)
        {
            if (score < best) {
                best = score;
                move = new_move;
            }
        }

        RaycastHit2D look_ahead = Physics2D.Raycast(pos, (next_pos - pos), (next_pos - pos).magnitude, AITerrainMask);

        if (look_ahead) {
            // we're hitting something
            if (Vector2.Angle(look_ahead.normal, Vector2.up) < steepestSlopeDegrees) {
                // we can stand on it
                (float, AIMoves) left_result = ExploreLateral(pos, false, depth + 1, max_depth);
                (float, AIMoves) right_result = ExploreLateral(pos, true, depth + 1, max_depth);
                CheckIfBetter(left_result.Item1, left_result.Item2);
                CheckIfBetter(right_result.Item1, right_result.Item2);
                return (best, move);
            } else {
                // give up
                return (best, move);
            }
        } else {
            // continue jump trajectory
            (float, AIMoves) result = ExploreJump(next_pos, next_vel, depth + 1, max_depth);
            CheckIfBetter(result.Item1, result.Item2);
            return (best, move);
        }
    }

    protected void DebugSquare(Vector3 pos, Color color)
    {
        Vector3 width = new Vector3(0.1f, 0f, 0f);
        Vector3 height = new Vector3(0f, 0.1f, 0f);
        Vector3 start = pos - width - height;
        Debug.DrawLine(start, start + width * 2, color, 0, false);
        Debug.DrawLine(start, start + height * 2, color, 0, false);
        Debug.DrawLine(start + width * 2, start + width * 2 + height * 2, color, 0, false);
        Debug.DrawLine(start + height * 2, start + width * 2 + height * 2, color, 0, false);
    }

    protected void DebugDiamond(Vector3 pos, Color color)
    {
        Vector3 width = new Vector3(0.1f, 0f, 0f);
        Vector3 height = new Vector3(0f, 0.1f, 0f);
        Debug.DrawLine(pos - height, pos - width, color, 0, false);
        Debug.DrawLine(pos - height, pos + width, color, 0, false);
        Debug.DrawLine(pos - width, pos + height, color, 0, false);
        Debug.DrawLine(pos + width, pos + height, color, 0, false);
    }

    protected void DebugCircle(Vector3 pos, Color color)
    {
        float angle_change = Mathf.PI / 8;
        float radius = 0.1f;
        for (float theta = 0; theta < 2 * Mathf.PI; theta += angle_change) {
            Vector3 start = pos + new Vector3(Mathf.Cos(theta), Mathf.Sin(theta), 0f) * radius;
            Vector3 end = pos + new Vector3(Mathf.Cos(theta + angle_change), Mathf.Sin(theta + angle_change), 0f) * radius;
            Debug.DrawLine(start, end, color, 0, false);
        }
    }

    protected void AddVelocity(Vector2 vel)
    {
        rb.velocity += vel;
    }

    public virtual void DoAbilityPrimaryDown(object sender, Vector3Args args)
    {
        // override with cool stuff
    }
    
    public virtual void DoAbilityPrimaryHold(object sender, Vector3Args args)
    {
        // override with cool stuff
    }
    
    public virtual void DoAbilityPrimaryUp(object sender, Vector3Args args)
    {
        // override with cool stuff
    }
    
    public virtual void OnMoveToDown(object sender, Vector3Args args)
    {
        // override with cool stuff
    }
    public virtual void OnMoveToUp(object sender, Vector3Args args)
    {
        // override with cool stuff
    }

    public bool GetOnGround()
    {
        return onGround;
    }

}
