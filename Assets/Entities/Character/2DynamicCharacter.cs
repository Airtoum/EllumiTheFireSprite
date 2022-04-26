using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CollisionData))]

public class BackupDynamicCharacter : Character
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
        ExplorePlatform();
    }

    protected (float, List<(Vector2, int)>) ExplorePlatform()
    {
        Vector2 pos = cl.bounds.center;
        Vector2 vel = rb.velocity;
        
        float best_fitness = Vector2.Distance(pos, AIDestination);
        List<(Vector2, int)> best_path = new List<(Vector2, int)>(); best_path.Add((pos, 0));
        
        void CheckIfBetter(float fitness, List<(Vector2, int)> new_path)
        {
            if (fitness < best_fitness) {
                best_fitness = fitness;
                best_path = new_path;
            }
        }

        (float left_fitness, List<(Vector2, int)> left_path) = ExploreLateral(pos, vel, false, 0, AIMaxDepth);
        CheckIfBetter(left_fitness, left_path);
        (float right_fitness, List<(Vector2, int)> right_path) = ExploreLateral(pos, vel, true, 0, AIMaxDepth);
        CheckIfBetter(right_fitness, right_path);
        if (onGround) {
            (float jump_left_fitness, List<(Vector2, int)> jump_left_path) = ExploreAerial(pos, vel, -1, 0, AIMaxDepth);
            CheckIfBetter(jump_left_fitness, jump_left_path);
            (float jump_up_fitness, List<(Vector2, int)> jump_up_path) = ExploreAerial(pos, vel, 0, 0, AIMaxDepth);
            CheckIfBetter(jump_up_fitness, jump_up_path);
            (float jump_right_fitness, List<(Vector2, int)> jump_right_path) = ExploreAerial(pos, vel, 1, 0, AIMaxDepth);
            CheckIfBetter(jump_right_fitness, jump_right_path);
        }

        inputFlags = 0;
        if (best_path.Count > 0) {
            inputFlags = best_path[0].Item2;
        }

        return (best_fitness, best_path);
    }

    protected RaycastHit2D TripleRaycast(Vector2 center, Vector2 offset, Vector2 direction, float max_length, int layer_mask)
    {
        RaycastHit2D result_hit;
        RaycastHit2D hit1 = Physics2D.Raycast(center, direction, max_length, layer_mask);
        RaycastHit2D hit2 = Physics2D.Raycast(center + offset, direction, max_length, layer_mask);
        RaycastHit2D hit3 = Physics2D.Raycast(center - offset, direction, max_length, layer_mask);
        if (hit1 && hit1.distance < hit2.distance && hit1.distance < hit3.distance) {
            result_hit = hit1;
        } else if (hit2 && hit2.distance < hit3.distance) {
            result_hit = hit2;
        } else {
            result_hit = hit3;
        }
        return result_hit;
    }

    // returns optimal fitness, then approximate list of inputs to get there
    protected (float, List<(Vector2, int)>) ExploreLateral(Vector2 pos, Vector2 vel, bool is_right, int depth, int max_depth)
    {
        float best_fitness = Vector2.Distance(pos, AIDestination);
        List<(Vector2, int)> node_inputs = new List<(Vector2, int)>(); node_inputs.Add((pos, 0));
        List<(Vector2, int)> best_future_path = new List<(Vector2, int)>();

        if (depth > max_depth) {
            return (best_fitness, node_inputs);
        }

        void CheckIfBetter(float fitness, List<(Vector2, int)> new_node_inputs, List<(Vector2, int)> new_future_path)
        {
            if (fitness < best_fitness) {
                best_fitness = fitness;
                node_inputs = new_node_inputs;
                best_future_path = new_future_path;
            }
        }

        float collider_width = cl.bounds.size.x;
        float collider_height = cl.bounds.size.y;
        int input_lr = is_right ? INPUT_RIGHT : INPUT_LEFT;

        RaycastHit2D look_down = TripleRaycast(pos, Vector2.left * collider_width/2f, Vector2.down, AICliffDownwarpDistance, AITerrainMask);

        if (look_down) {
            // there is floor beneath us
            vel.y = 0;
            
            // let's snap to be an appropriate distance to it (for slopes)
            pos = look_down.point + (Vector2.up * collider_height * 0.5f);
            Vector2 original_pos = pos;
            Vector2 original_vel = vel;
            
            // let's continue walking
            Vector2 next_pos = pos;
            Vector2 next_vel = vel;
            List<(Vector2, int)> walk_node_inputs = new List<(Vector2, int)>();
            List<(Vector2, int)> jump_node_inputs = new List<(Vector2, int)>(); jump_node_inputs.Add((pos, INPUT_JUMP | input_lr));
            for (int i = 0; i < AIPhysicsSteps; i++) {
                node_inputs.Add((next_pos, input_lr));
                next_vel.x = next_vel.x * (1 - tightness) + (is_right ? moveSpeed : -moveSpeed) * tightness;
                next_pos += vel;
            }
            
            RaycastHit2D look_right = Physics2D.Raycast(pos, is_right ? Vector2.right : Vector2.left, (next_pos - pos).magnitude, AITerrainMask);

            if (look_right) {
                // there's something to the right of us; let's assume its a wall
                
                // let's re-do the movement, to find out where to stop)
                next_pos = pos;
                next_vel = vel;
                int i = 0;
                List<(Vector2, int)> wall_node_inputs = new List<(Vector2, int)>();
                for (i = 0; i < AIPhysicsSteps; i++) {
                    wall_node_inputs.Add((next_pos, input_lr));
                    next_vel.x = next_vel.x * (1 - tightness) + (is_right ? moveSpeed : -moveSpeed) * tightness;
                    next_pos += vel;
                    if ((next_pos - pos).magnitude >= look_right.distance - collider_width/2f) {
                        break;
                    }
                }
                pos = next_pos;
                vel = new Vector2(0, next_vel.y);
                (float wall_fitness, List<(Vector2, int)> wall_path) = ExploreWall(pos, vel, is_right, depth + 1, max_depth);
                CheckIfBetter(wall_fitness, wall_node_inputs, wall_path);
            } else {
                // there's nothing to our right
                pos = next_pos;
                vel = new Vector2(next_vel.x, 0);
                
                RaycastHit2D look_down_again = Physics2D.Raycast(pos, Vector2.down, AICliffDownwarpDistance, AITerrainMask);
                if (look_down_again) {
                    // we're still on ground
                    
                    // let's snap to be an appropriate distance to it (for slopes)
                    pos = look_down_again.point + (Vector2.up * collider_height * 0.5f);
                    
                    (float walk_fitness, List<(Vector2, int)> walk_path) = ExploreLateral(pos, vel, is_right, depth + 1, max_depth);
                    CheckIfBetter(walk_fitness, walk_node_inputs, walk_path);
                } else {
                    // we will not be on ground anymore, but we should be able to jump
                    // what if we fall off? (slightly inaccurate)
                    (float fall_fitness, List<(Vector2, int)> fall_path) = ExploreAerial(pos, vel, is_right ? 1 : -1, depth + 1, max_depth);
                    CheckIfBetter(fall_fitness, walk_node_inputs, fall_path);
                    // what if we jump off?
                    (float jump_fitness, List<(Vector2, int)> jump_path) = ExploreAerial(original_pos, new Vector2(vel.x, jumpAmount), is_right ? 1 : -1, depth + 1, max_depth);
                    CheckIfBetter(jump_fitness, jump_node_inputs, jump_path);
                }
            }
        } else {
            // we aren't on the ground (we might not have coyote time, either)
            List<(Vector2, int)> fall_inputs = new List<(Vector2, int)>();
            fall_inputs.Add((pos, input_lr));
            (float fall_fitness, List<(Vector2, int)> fall_path) = ExploreAerial(pos, vel, is_right ? 1 : -1, depth + 1, max_depth);
            CheckIfBetter(fall_fitness, fall_inputs, fall_path);
        }

        List<(Vector2, int)> final_path = new List<(Vector2, int)>();
        final_path.AddRange(node_inputs);
        final_path.AddRange(best_future_path);
        return (best_fitness, final_path);
    }

    protected (float, List<(Vector2, int)>) ExploreWall(Vector2 pos, Vector2 vel, bool is_right, int depth, int max_depth)
    {
        float best_fitness = Vector2.Distance(pos, AIDestination);
        List<(Vector2, int)> node_inputs = new List<(Vector2, int)>(); node_inputs.Add((pos, 0));
        List<(Vector2, int)> best_future_path = new List<(Vector2, int)>();

        if (depth > max_depth) {
            return (best_fitness, node_inputs);
        }
        
        void CheckIfBetter(float fitness, List<(Vector2, int)> new_node_inputs, List<(Vector2, int)> new_future_path)
        {
            if (fitness < best_fitness) {
                best_fitness = fitness;
                node_inputs = new_node_inputs;
                best_future_path = new_future_path;
            }
        }

        float collider_width = cl.bounds.size.x;
        float collider_height = cl.bounds.size.y;
        int input_lr = is_right ? INPUT_RIGHT : INPUT_LEFT;
        
        // double check; are we still at a wall? try moving right
        
        Vector2 next_pos = pos;
        Vector2 next_vel = vel;
        List<(Vector2, int)> walk_node_inputs = new List<(Vector2, int)>();
        for (int i = 0; i < AIPhysicsSteps; i++) {
            walk_node_inputs.Add((next_pos, input_lr));
            next_vel.x = next_vel.x * (1 - tightness) + (is_right ? moveSpeed : -moveSpeed) * tightness;
            next_pos += vel;
        }

        RaycastHit2D look_right = TripleRaycast(pos, Vector2.down * collider_height/2f, is_right ? Vector2.right : Vector2.left, (next_pos - pos).magnitude, AITerrainMask);

        if (look_right) {
            // we are still at a wall, search in the direction we're moving (up or down)
            vel.x = 0;
            next_pos = pos;
            next_vel = vel;
            List<(Vector2, int)> wall_node_inputs = new List<(Vector2, int)>();
            for (int i = 0; i < AIPhysicsSteps; i++) {
                wall_node_inputs.Add((next_pos, input_lr));
                next_vel += gravitationalAcceleration * Time.fixedDeltaTime;
                next_pos += vel;
            }

            if (vel.y > 0) {
                // we're moving up
                RaycastHit2D look_up = Physics2D.Raycast(pos, Vector2.down, (next_pos - pos).magnitude, AITerrainMask);

                if (look_up && look_up.collider.gameObject.layer != LayerMask.NameToLayer("Semisolid")) {
                    // there's something above us
                    // who knows, we might drop down onto a semisolid platform
                    List<(Vector2, int)> ascend_node_inputs = new List<(Vector2, int)>();
                    for (int i = 0; i < AIPhysicsSteps; i++) {
                        ascend_node_inputs.Add((next_pos, input_lr));
                        next_vel += gravitationalAcceleration * Time.fixedDeltaTime;
                        next_pos += vel;
                        if ((next_pos - pos).magnitude >= look_up.distance - collider_height/2f) {
                            break;
                        }
                    }
                    pos = next_pos;
                    vel = Vector2.zero;
                    (float wall_fitness, List<(Vector2, int)> wall_path) = ExploreWall(pos, vel, is_right, depth + 1, max_depth);
                    CheckIfBetter(wall_fitness, ascend_node_inputs, wall_path);
                } else {
                    // there's nothing above us, continue upwards
                    pos = next_pos;
                    vel = next_vel;

                    (float wall_fitness, List<(Vector2, int)> wall_path) = ExploreWall(pos, vel, is_right, depth + 1, max_depth);
                    CheckIfBetter(wall_fitness, wall_node_inputs, wall_path);
                }
            } else {
                // we're moving down
                RaycastHit2D look_down = Physics2D.Raycast(pos, Vector2.down, (next_pos - pos).magnitude, AITerrainMask);

                if (look_down) {
                    // we're hitting ground
                    List<(Vector2, int)> fall_node_inputs = new List<(Vector2, int)>();
                    for (int i = 0; i < AIPhysicsSteps; i++) {
                        fall_node_inputs.Add((next_pos, input_lr));
                        next_vel += gravitationalAcceleration * Time.fixedDeltaTime;
                        next_pos += vel;
                        if ((next_pos - pos).magnitude >= look_down.distance - collider_height/2f) {
                            break;
                        }
                    }
                    pos = next_pos;
                    vel = Vector2.zero;
                    // we might have fallen someplace with stuff on the right or left
                    // this may backtrack but it should be manageable
                    RaycastHit2D land_look_left = Physics2D.Raycast(pos, Vector2.left, collider_width * 2, AITerrainMask);
                    RaycastHit2D land_look_right = Physics2D.Raycast(pos, Vector2.right, collider_width * 2, AITerrainMask);
                    if (!land_look_left) {
                        (float left_fitness, List<(Vector2, int)> left_path) = ExploreLateral(pos, vel, false, depth + 1, max_depth);
                        CheckIfBetter(left_fitness, fall_node_inputs, left_path);
                    }
                    if (!land_look_right) {
                        (float right_fitness, List<(Vector2, int)> right_path) = ExploreLateral(pos, vel, true, depth + 1, max_depth);
                        CheckIfBetter(right_fitness, fall_node_inputs, right_path);
                    }
                } else {
                    // there's nothing beneath us, continue downwards
                    pos = next_pos;
                    vel = next_vel;
                    
                    (float wall_fitness, List<(Vector2, int)> wall_path) = ExploreWall(pos, vel, is_right, depth + 1, max_depth);
                    CheckIfBetter(wall_fitness, wall_node_inputs, wall_path);
                }
            }
        } else {
            // good! there is no more wall! let's move right and then switch over to walk
            pos = next_pos;
            pos = next_vel;
            
            (float walk_fitness, List<(Vector2, int)> walk_path) = ExploreLateral(pos, vel, is_right, depth + 1, max_depth);
            CheckIfBetter(walk_fitness, walk_node_inputs, walk_path);
        }

        List<(Vector2, int)> final_path = new List<(Vector2, int)>();
        final_path.AddRange(node_inputs);
        final_path.AddRange(best_future_path);
        return (best_fitness, final_path);
    }

    
    protected (float, List<(Vector2, int)>) ExploreAerial(Vector2 pos, Vector2 vel, int direction_sign, int depth, int max_depth)
    {
        float best_fitness = Vector2.Distance(pos, AIDestination);
        List<(Vector2, int)> node_inputs = new List<(Vector2, int)>(); node_inputs.Add((pos, 0));
        List<(Vector2, int)> best_future_path = new List<(Vector2, int)>();

        if (depth > max_depth) {
            return (best_fitness, node_inputs);
        }
        
        void CheckIfBetter(float fitness, List<(Vector2, int)> new_node_inputs, List<(Vector2, int)> new_future_path)
        {
            if (fitness < best_fitness) {
                best_fitness = fitness;
                node_inputs = new_node_inputs;
                best_future_path = new_future_path;
            }
        }

        float collider_width = cl.bounds.size.x;
        float collider_height = cl.bounds.size.y;
        int input_lr = direction_sign == 0 ? 0 : (direction_sign == 1 ? INPUT_RIGHT : INPUT_LEFT);
        
        // find next expected position
        Vector2 next_pos = pos;
        Vector2 next_vel = vel;
        List<(Vector2, int)> aerial_inputs = new List<(Vector2, int)>();
        for (int i = 0; i < AIPhysicsSteps; i++) {
            aerial_inputs.Add((next_pos, input_lr));
            next_vel.x = next_vel.x * (1 - tightness) + (direction_sign * moveSpeed) * tightness;
            next_vel += gravitationalAcceleration * Time.fixedDeltaTime;
            next_pos += vel;
        }

        RaycastHit2D look_ahead = Physics2D.Raycast(pos, (next_pos - pos).normalized, (next_pos - pos).magnitude, AITerrainMask);

        if (look_ahead) {
            // we hit something
            float collision_angle = Vector2.Angle(look_ahead.normal, Vector2.up);
            if (collision_angle < steepestSlopeDegrees) {
                // it's standable
                List<(Vector2, int)> landing_inputs = new List<(Vector2, int)>();
                for (int i = 0; i < AIPhysicsSteps; i++) {
                    landing_inputs.Add((next_pos, input_lr));
                    next_vel.x = next_vel.x * (1 - tightness) + (direction_sign * moveSpeed) * tightness;
                    next_vel += gravitationalAcceleration * Time.fixedDeltaTime;
                    next_pos += vel;
                    if ((next_pos - pos).magnitude >= look_ahead.distance - collider_height / 2f) {
                        break;
                    }
                }
                pos = next_pos;
                vel = new Vector2(next_vel.x, 0);
                // could be either left or right, we might have landed somewhere new
                RaycastHit2D land_look_left = Physics2D.Raycast(pos, Vector2.left, collider_width * 2, AITerrainMask);
                RaycastHit2D land_look_right = Physics2D.Raycast(pos, Vector2.right, collider_width * 2, AITerrainMask);
                if (!land_look_left){
                    (float left_fitness, List<(Vector2, int)> left_path) = ExploreLateral(pos, vel, false, depth + 1, max_depth);
                    CheckIfBetter(left_fitness, landing_inputs, left_path);
                }
                if (!land_look_right){
                    (float right_fitness, List<(Vector2, int)> right_path) = ExploreLateral(pos, vel, true, depth + 1, max_depth);
                    CheckIfBetter(right_fitness, landing_inputs, right_path);
                }
            } else if (collision_angle < 91) {
                // probably a wall
                List<(Vector2, int)> wall_inputs = new List<(Vector2, int)>();
                for (int i = 0; i < AIPhysicsSteps; i++) {
                    wall_inputs.Add((next_pos, input_lr));
                    next_vel.x = next_vel.x * (1 - tightness) + (direction_sign * moveSpeed) * tightness;
                    next_vel += gravitationalAcceleration * Time.fixedDeltaTime;
                    next_pos += vel;
                    if ((next_pos - pos).magnitude >= look_ahead.distance - collider_width / 2f) {
                        break;
                    }
                }
                pos = next_pos;
                vel = new Vector2(0, next_vel.y);
                (float wall_fitness, List<(Vector2, int)> wall_path) = ExploreWall(pos, vel, direction_sign>=0, depth + 1, max_depth);
                CheckIfBetter(wall_fitness, wall_inputs, wall_path);
            } else {
                // a ceiling
                List<(Vector2, int)> bonk_inputs = new List<(Vector2, int)>();
                for (int i = 0; i < AIPhysicsSteps; i++) {
                    bonk_inputs.Add((next_pos, input_lr));
                    next_vel.x = next_vel.x * (1 - tightness) + (direction_sign * moveSpeed) * tightness;
                    next_vel += gravitationalAcceleration * Time.fixedDeltaTime;
                    next_pos += vel;
                    if ((next_pos - pos).magnitude >= look_ahead.distance - collider_height / 2f) {
                        break;
                    }
                }
                pos = next_pos;
                vel = new Vector2(next_vel.x, 0);
                (float bonk_fitness, List<(Vector2, int)> bonk_path) = ExploreAerial(pos, vel, direction_sign, depth + 1, max_depth);
                CheckIfBetter(bonk_fitness, bonk_inputs, bonk_path);
            }
        } else {
            // we didn't hit anything, do next jump step
            pos = next_pos;
            vel = next_vel;
            (float aerial_fitness, List<(Vector2, int)> aerial_path) = ExploreAerial(pos, vel, direction_sign, depth + 1, max_depth);
            CheckIfBetter(aerial_fitness, aerial_inputs, aerial_path);
        }
        
        List<(Vector2, int)> final_path = new List<(Vector2, int)>();
        final_path.AddRange(node_inputs);
        final_path.AddRange(best_future_path);
        return (best_fitness, final_path);
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
