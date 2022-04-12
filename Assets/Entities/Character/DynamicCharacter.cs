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
        Stop, Lateral, Up, Down, LateralUp, JumpSideways, JumpUpwards, JumpBackwards
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

    protected virtual void AIInput()
    {
        if (!AIActivated) return;
        if (Vector2.Distance(transform.position, AIDestination) <= AICloseEnoughDistance) {
            AIActivated = false;
            inputFlags = 0;
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
        debug_AINodeCount = 0;
        debug_AILandFromJumpCount = 0;
        debug_AIWalksCanceled = 0;
        debug_WalkNodes = 0;
        debug_WallNodes = 0;
        debug_JumpNodes = 0;
        AIVisited.Clear();
        
        Vector2 pos = transform.position;
        pos.x = Mathf.Round(pos.x * AIGridFineness) / AIGridFineness;
        pos.y = Mathf.Round(pos.y * AIGridFineness) / AIGridFineness;
        float jump_vel = (onGround || coyoteTimeTimer <= coyoteTime) && jumpCooldownTimer > jumpCooldown
            ? jumpAmount
            : rb.velocity.y;
        
        float dist = Vector2.Distance(pos, AIDestination);
        int lateral_choice = 0;
        int backward_choice = 0;
        List<(Vector2, AIMoves)> chosen_path = new List<(Vector2, AIMoves)>();
        AIMoves selected_input = AIMoves.Stop;
        float best_score = float.MaxValue;
        void FindBest((float, AIMoves, List<(Vector2, AIMoves)>) search, int lat_chc, int bak_chc)
        {
            if (search.Item1 < best_score) {
                best_score = search.Item1;
                selected_input = search.Item2;
                chosen_path = search.Item3;
                lateral_choice = lat_chc;
                backward_choice = bak_chc;
            }
        }
        if (onGround) {
            (float, AIMoves, List<(Vector2, AIMoves)>) left = ExploreLateral(pos, false, 0, AIMaxDepth);
            (float, AIMoves, List<(Vector2, AIMoves)>) right = ExploreLateral(pos, true, 0, AIMaxDepth);
            FindBest(left, INPUT_LEFT, INPUT_RIGHT);
            FindBest(right, INPUT_RIGHT, INPUT_LEFT);
        } else {
            (float, AIMoves, List<(Vector2, AIMoves)>) jump = ExploreJump(pos, jump_vel, 0, 0, AIMaxDepth, true);
            (float, AIMoves, List<(Vector2, AIMoves)>) jump_left = ExploreJump(pos, jump_vel, -1, 0, AIMaxDepth, true);
            (float, AIMoves, List<(Vector2, AIMoves)>) jump_right = ExploreJump(pos, jump_vel, 1, 0, AIMaxDepth, true);
            (float, AIMoves, List<(Vector2, AIMoves)>) fall_left = ExploreJump(pos, rb.velocity.y, -1, 0, AIMaxDepth, false);
            (float, AIMoves, List<(Vector2, AIMoves)>) fall_right = ExploreJump(pos, rb.velocity.y, 1, 0, AIMaxDepth, false);
            FindBest(jump, 0, 0);
            FindBest(jump_left, INPUT_LEFT, INPUT_RIGHT);
            FindBest(jump_right, INPUT_RIGHT, INPUT_LEFT);
            FindBest(fall_left, INPUT_LEFT, INPUT_RIGHT);
            FindBest(fall_right, INPUT_RIGHT, INPUT_LEFT);
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
            case AIMoves.JumpSideways:
                inputFlags = INPUT_JUMP | lateral_choice;
                break;
            case AIMoves.JumpUpwards:
                inputFlags = INPUT_JUMP;
                break;
            case AIMoves.JumpBackwards:
                inputFlags = INPUT_JUMP | backward_choice;
                break;
            default:
                inputFlags = 0;
                break;
        }

        for(int i = 0; i < chosen_path.Count - 1; i++) {
            Vector2 start = chosen_path[i].Item1;
            Vector2 end = chosen_path[i + 1].Item1;
            Debug.DrawLine(start, end, Color.white);
        }
        DebugSquare(pos, Color.white);
        DebugDiamond(AIDestination, Color.white);

        if (best_score > dist - AICloseEnoughDistance) {
            AIActivated = false;
            inputFlags = 0;
        }
        
        /*debug_text.text = "Node count " + debug_AINodeCount + "\n" +
                          "Landings from jump " + debug_AILandFromJumpCount + "\n" +
                          "Dictionary size " + AIVisited.Count + "\n" +
                          "Input " + inputFlags + "\n" +
                          "Walks Canceled " + debug_AIWalksCanceled + "\n" +
                          "Walk Nodes " + debug_WalkNodes + "\n" +
                          "Wall Nodes " + debug_WallNodes + "\n" +
                          "Jump Nodes " + debug_JumpNodes + "\n" +
                          "Best Score " + best_score;*/
    }

    // we don't want a long path, so lower depth is better
    protected float ScoreHeuristic(float distance, int depth)
    {
        return distance + (depth * AIDepthPenalty);
    }

    protected bool HaveWeDoneBetter(Vector2 pos, AISearchStates state, int depth)
    {
        
        pos.x = Mathf.Round(pos.x * AIGridFineness);
        pos.y = Mathf.Round(pos.y * AIGridFineness);
        (Vector2, AISearchStates) key = (pos, AISearchStates.Lateral); 
        if (AIVisited.ContainsKey(key)) {
            if (AIVisited[key] < depth) {
                return true;
            }
        }
        return false;
    }

    protected void MarkVisited(Vector2 pos, AISearchStates state, int depth)
    {
        return;
        pos.x = Mathf.Floor(pos.x * AIGridFineness);
        pos.y = Mathf.Floor(pos.y * AIGridFineness);
        (Vector2, AISearchStates) key = (pos, AISearchStates.Lateral);
        if (AIVisited.ContainsKey(key)) {
            AIVisited[key] = Mathf.Min(AIVisited[key], depth);
        } else {
            AIVisited[key] = depth;
        }
    }
    
    // returns how close it gets to AIDestination on this path
    protected (float, AIMoves, List<(Vector2, AIMoves)>) ExploreLateral(Vector2 pos, bool is_right, int depth, int max_depth)
    {
        debug_AINodeCount += 1;
        debug_WalkNodes += 1;
        
        float best = Vector2.Distance(pos, AIDestination);
        AIMoves move = AIMoves.Stop;
        List<(Vector2, AIMoves)> path = new List<(Vector2, AIMoves)>();
        path.Add((pos, move));

        if (depth > max_depth) return (best, move, path);
        if (HaveWeDoneBetter(pos, AISearchStates.Lateral, depth)) {
            debug_AIWalksCanceled += 1;
            return (float.MaxValue, move, path);
        }
        MarkVisited(pos, AISearchStates.Lateral, depth);
            
        float dx = (is_right ? 1f : -1f) * cl.bounds.size.x;
        float dy = cl.bounds.size.y;

        void CheckIfBetter(float score, AIMoves action, List<(Vector2, AIMoves)> trace)
        {
            if (score < best) {
                best = Mathf.Max(score, AICloseEnoughDistance);
                move = action;
                path = trace;
            }
        }
        
        RaycastHit2D down = Physics2D.Raycast(pos, Vector2.down, AICliffDropDistance, AITerrainMask);
        RaycastHit2D down_left = Physics2D.Raycast(pos + new Vector2(-cl.bounds.extents.x, 0), Vector2.down, AICliffDropDistance, AITerrainMask);
        RaycastHit2D down_right = Physics2D.Raycast(pos + new Vector2(cl.bounds.extents.x, 0), Vector2.down, AICliffDropDistance, AITerrainMask);
        if (down) {
            // there is ground beneath us
            float highest_floor = down.distance;
            if (down_left) highest_floor = Mathf.Min(highest_floor, down_left.distance);
            if (down_right) highest_floor = Mathf.Min(highest_floor, down_right.distance);
            if (highest_floor <= AICliffDownwarpDistance) {
                // we're still on the ground
                Vector2 adjusted_pos = pos + Vector2.down * (highest_floor - cl.bounds.extents.y);
                best = Mathf.Min(best, Ultramath.DistanceToLineSegment(AIDestination, pos, adjusted_pos));
                pos = adjusted_pos;
                if (HaveWeDoneBetter(pos, AISearchStates.Lateral, depth)) {
                    debug_AIWalksCanceled += 1;
                    return (float.MaxValue, move, path);
                }

                RaycastHit2D lateral = Physics2D.Raycast(pos, new Vector2(dx, 0f), Mathf.Abs(dx), AITerrainMask);
                if (lateral) {
                    // there's something to our side
                    Debug.DrawLine(lateral.point, lateral.point + lateral.normal, Color.yellow);
                    if (Mathf.Abs(Vector2.Angle(lateral.normal, Vector2.up)) <= steepestSlopeDegrees) {
                        // we've hit a slope, repeat higher
                        (float, AIMoves, List<(Vector2, AIMoves)>) result =
                            ExploreLateral(pos + new Vector2(dx, cl.bounds.size.y), is_right, depth, max_depth);
                        DebugSquare(pos, Color.yellow);
                        CheckIfBetter(result.Item1, result.Item2, result.Item3);
                    } else {
                        // we've hit a wall, Explore Wall
                        (float, AIMoves, List<(Vector2, AIMoves)>) result = ExploreWall(pos, is_right, depth, max_depth,
                            0);
                        DebugSquare(pos, Color.magenta);
                        CheckIfBetter(result.Item1, AIMoves.LateralUp, result.Item3);
                    }
                } else {
                    // keep going
                    DebugSquare(pos, Color.green);
                    Vector2 adjusted_pos_again = pos + new Vector2(dx, 0f);
                    best = Mathf.Min(best, Ultramath.DistanceToLineSegment(AIDestination, pos, adjusted_pos_again));
                    pos = adjusted_pos_again;
                    (float, AIMoves, List<(Vector2, AIMoves)>) result_walk = ExploreLateral(pos, is_right, depth + 1, max_depth);
                    (float, AIMoves, List<(Vector2, AIMoves)>) result_jump_forward = ExploreJump(pos, jumpAmount, (is_right ? 1 : -1), depth + 1, max_depth);
                    //(float, AIMoves, List<(Vector2, AIMoves)>) result_jump_up = ExploreJump(pos, jumpAmount, 0, depth + 1, max_depth);
                    (float, AIMoves, List<(Vector2, AIMoves)>) result_jump_back = ExploreJump(pos, jumpAmount, (is_right ? -1 : 1), depth + 1, max_depth);
                    CheckIfBetter(result_walk.Item1, AIMoves.Lateral, result_walk.Item3);
                    CheckIfBetter(result_jump_forward.Item1 + AIJumpPenalization, AIMoves.JumpSideways, result_jump_forward.Item3);
                    //CheckIfBetter(result_jump_up.Item1 + AIJumpPenalization, AIMoves.JumpUpwards);
                    CheckIfBetter(result_jump_back.Item1 + AIJumpPenalization, AIMoves.JumpBackwards, result_jump_back.Item3);
                }
            } else {
                // the ground is kind of far away, we would be airborne
                DebugSquare(pos, new Color(1, 0.5f, 0.5f));
                (float, AIMoves, List<(Vector2, AIMoves)>) result = ExploreJump(pos, 0, (is_right ? 1 : -1), depth + 1, max_depth);
                CheckIfBetter(result.Item1, AIMoves.Lateral, result.Item3);
            }
        } else {
            // there was no ground found, this is an edge
            DebugSquare(pos, Color.red);
            (float, AIMoves, List<(Vector2, AIMoves)>) result = ExploreJump(pos, jumpAmount, (is_right ? 1 : -1), depth + 1, max_depth);
            CheckIfBetter(result.Item1, AIMoves.JumpSideways, result.Item3);
        }

        List<(Vector2, AIMoves)> final_path = new List<(Vector2, AIMoves)>();
        final_path.Add((pos, move));
        final_path.AddRange(path);
        return (best, move, final_path);
    }

    protected (float, AIMoves, List<(Vector2, AIMoves)>) ExploreWall(Vector2 pos, bool is_right, int depth, int max_depth, int wall_depth)
    {
        debug_AINodeCount += 1;
        debug_WallNodes += 1;
        float dist = Vector2.Distance(pos, AIDestination);
        float best = Mathf.Min(dist);
        AIMoves move = AIMoves.Stop;
        List<(Vector2, AIMoves)> path = new List<(Vector2, AIMoves)>();
        path.Add((pos, move));

        if (depth > max_depth) return (dist, AIMoves.Stop, path);
        if (HaveWeDoneBetter(pos, AISearchStates.Wall, depth)) return (float.MaxValue, AIMoves.Stop, path);
        MarkVisited(pos, AISearchStates.Wall, depth);
        float max_jump_height = - jumpAmount * jumpAmount / (2 * gravitationalAcceleration.y);
        float dy = cl.bounds.size.y;
        float dx = (is_right ? 1f : -1f) * cl.bounds.size.x;
        int max_wall_depth = Mathf.RoundToInt(max_jump_height / dy); 
        if (wall_depth > max_wall_depth) return (dist, AIMoves.Stop, path);
        
        void CheckIfBetter(float score, AIMoves action, List<(Vector2, AIMoves)> trace)
        {
            if (score < best) {
                best = score;
                move = action;
                path = trace;
            }
        }

        RaycastHit2D lateral = Physics2D.Raycast(pos, new Vector2(dx, 0f), Mathf.Abs(dx), AITerrainMask);
        if (lateral) {
            // the wall is still there
            RaycastHit2D up = Physics2D.Raycast(pos, Vector2.up, dy, AITerrainMask);
            if (up) {
                // there is a ceiling
                DebugDiamond(pos, Color.red);
                ;
            } else {
                // the wall continues up
                (float, AIMoves, List<(Vector2, AIMoves)>) result = ExploreWall(pos + new Vector2(0f, dy), is_right, depth + 1, max_depth, wall_depth + 1);
                DebugDiamond(pos, Color.magenta);
                CheckIfBetter(result.Item1, AIMoves.LateralUp, result.Item3);
            }
        } else {
            // the wall is gone, implies ground
            (float, AIMoves, List<(Vector2, AIMoves)>) result = ExploreLateral(pos + new Vector2(dx, 0f), is_right, depth + 1, max_depth);
            DebugDiamond(pos, Color.green);
            CheckIfBetter(result.Item1, AIMoves.Lateral, result.Item3);
        }

        List<(Vector2, AIMoves)> final_path = new List<(Vector2, AIMoves)>();
        final_path.Add((pos, move));
        final_path.AddRange(path);
        return (best, move, final_path);
    }

    
    protected (float, AIMoves, List<(Vector2, AIMoves)>) ExploreJump(Vector2 pos, float expected_vert_vel, int direction_sign, int depth, int max_depth, bool first = true)
    {
        debug_AINodeCount += 1;
        debug_JumpNodes += 1;

        float best = ScoreHeuristic(Vector2.Distance(pos, AIDestination), depth);
        AIMoves move = first ? AIMoves.JumpSideways : AIMoves.Stop;
        List<(Vector2, AIMoves)> path = new List<(Vector2, AIMoves)>();
        path.Add((pos, move));
        
        if (depth > max_depth) return (best, AIMoves.Stop, path);
        if (HaveWeDoneBetter(pos, AISearchStates.Jump, depth)) return (float.MaxValue, move, path);
        MarkVisited(pos, AISearchStates.Jump, depth);
        
        void CheckIfBetter(float score, AIMoves action, List<(Vector2, AIMoves)> trace)
        {
            if (score < best) {
                best = score;
                move = action;
                path = trace;
            }
        }

        float timestep = AIJumpTimestep;
        Vector2 lateral_move = direction_sign * Vector2.right;
        float gravity = gravitationalAcceleration.y;
        Vector2 next_approx_pos = pos +
                                  Ultramath.TrajectoryPos(expected_vert_vel, gravity , timestep) * Vector2.up + 
                                  moveSpeed * lateral_move * timestep;
        float next_approx_vel = Ultramath.TrajectoryVel(expected_vert_vel, gravity, timestep);
        Vector2 displacement = next_approx_pos - pos;
        RaycastHit2D jump = Physics2D.Raycast(pos, displacement, displacement.magnitude, AITerrainMask);
        RaycastHit2D jump_left = Physics2D.Raycast(pos + new Vector2(-cl.bounds.extents.x, 0), displacement, displacement.magnitude, AITerrainMask);
        RaycastHit2D jump_right = Physics2D.Raycast(pos + new Vector2(cl.bounds.extents.x, 0), displacement, displacement.magnitude, AITerrainMask);
        if (jump || jump_left || jump_right) {
            DebugDiamond(jump.point, new Color(1, 0.5f, 0.5f));
            // we hit something
            bool can_stand_on = true;
            if (jump) {
                can_stand_on = can_stand_on && Mathf.Abs(Vector2.Angle(jump.normal, Vector2.up)) <= steepestSlopeDegrees;
                best = Mathf.Min(best, ScoreHeuristic(Ultramath.DistanceToLineSegment(AIDestination, pos, jump.point), depth));
            }
            if (jump_left) {
                can_stand_on = can_stand_on && Mathf.Abs(Vector2.Angle(jump_left.normal, Vector2.up)) <= steepestSlopeDegrees;
                best = Mathf.Min(best, ScoreHeuristic(Ultramath.DistanceToLineSegment(AIDestination, pos, jump_left.point), depth));
            }
            if (jump_right) {
                can_stand_on = can_stand_on && Mathf.Abs(Vector2.Angle(jump_right.normal, Vector2.up)) <= steepestSlopeDegrees;
                best = Mathf.Min(best, ScoreHeuristic(Ultramath.DistanceToLineSegment(AIDestination, pos, jump_right.point), depth));
            }
            if (can_stand_on)
            {
                // we can stand on it
                debug_AILandFromJumpCount += 1;
                (float, AIMoves, List<(Vector2, AIMoves)>) result_l = ExploreLateral(jump.point + jump.normal, false, depth + 1, max_depth);
                (float, AIMoves, List<(Vector2, AIMoves)>) result_r = ExploreLateral(jump.point + jump.normal, true, depth + 1, max_depth);
                CheckIfBetter(result_l.Item1, AIMoves.Lateral, result_l.Item3);
                CheckIfBetter(result_r.Item1, AIMoves.Lateral, result_r.Item3);
            } else {
                // we cannot stand on it
                ;
            }
        } else {
            // next jump step
            DebugDiamond(next_approx_pos, Color.red);
            Debug.DrawLine(pos, next_approx_pos, Color.red);
            (float, AIMoves, List<(Vector2, AIMoves)>) result = ExploreJump(next_approx_pos, next_approx_vel, direction_sign, depth + 1, max_depth, false);
            if (direction_sign == 0) {
                CheckIfBetter(result.Item1, first ? AIMoves.JumpUpwards : AIMoves.Stop, result.Item3);
            } else {
                CheckIfBetter(result.Item1, first ? AIMoves.JumpSideways : AIMoves.Lateral, result.Item3);
            }
        }
        
        List<(Vector2, AIMoves)> final_path = new List<(Vector2, AIMoves)>();
        final_path.Add((pos, move));
        final_path.AddRange(path);
        return (best, move, final_path);
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
