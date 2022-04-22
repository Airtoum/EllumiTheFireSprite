using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class AirSprite : MainCharacter
{

    [SerializeField] protected float AIReachedWallDistance = 1.2f;

    public GameObject windSpawn;
    public Vector3 firstPoint;
    public Vector3 secondPoint;
    public bool startNewWind = true;

    protected new void Awake()
    {
        GameEvents.OnPrimaryAbilityDown += DoAbilityPrimaryDown;
        
        base.Awake();
    }
    
    protected override void DoMovement()
    {
        Vector2 velocity = rb.velocity;
        
        float horizontal_movement = velocity.x;
        float vertical_movement = velocity.y;
        Vector2 target_movement = Vector2.zero;
        target_movement += ((inputFlags & INPUT_RIGHT) > 0) ? Vector2.right : Vector2.zero;
        target_movement += ((inputFlags & INPUT_LEFT) > 0) ? Vector2.left : Vector2.zero;
        target_movement += ((inputFlags & INPUT_UP) > 0) ? Vector2.up : Vector2.zero;
        target_movement += ((inputFlags & INPUT_DOWN) > 0) ? Vector2.down : Vector2.zero;
        target_movement = target_movement.normalized * moveSpeed;

        velocity = velocity * (1 - tightness) + target_movement * tightness;
        
        rb.velocity = velocity;
    }

    protected override void AIInput()
    {
        if (!AIActivated) return;
        if (Vector2.Distance(transform.position, AIDestination) <= AICloseEnoughDistance) {
            AIActivated = false;
            inputFlags = 0;
            return;
        }

        ExploreAir();
    }

    protected void ExploreAir()
    {
        Vector2 position = transform.position;
        (float, Vector2, List<(Vector2, Vector2)>) result = ExploreFly(transform.position, true, true, null, 0, 20);
        Vector2 move_vector = result.Item2;
        List<(Vector2, Vector2)> chosen_path = result.Item3;
        float angle = -Vector2.SignedAngle(move_vector, Vector2.right) * Mathf.Deg2Rad;

        int skip_to_index = 0;
        for(int i = chosen_path.Count - 1; i >= 0; i--) {
            Vector2 point = chosen_path[i].Item1;
            if (ExploreShortcuts(point)) {
                move_vector = (point - position).normalized;
                break;
            }
        }
        chosen_path.RemoveRange(0, skip_to_index);
        
        DebugText.SetText(chosen_path.Count + "\n" + result.Item1);
        for(int i = 0; i < chosen_path.Count - 1; i++) {
            Vector2 start = chosen_path[i].Item1;
            Vector2 end = chosen_path[i + 1].Item1;
            Debug.DrawLine(start, end, Color.white);
        }
        
        /*
        // this is disgusting but I can't think of a better way right now
        if (angle < -(7f / 8f) * Mathf.PI) {
            // left
            inputFlags = INPUT_LEFT;
        } else if (angle < -(5f / 8f) * Mathf.PI) {
            // down-left
            inputFlags = INPUT_LEFT | INPUT_DOWN;
        } else if (angle < -(3f / 8f) * Mathf.PI) {
            // down
            inputFlags = INPUT_DOWN;
        } else if (angle < -(1f / 8f) * Mathf.PI) {
            // down-right
            inputFlags = INPUT_RIGHT | INPUT_DOWN;
        } else if (angle < (1f / 8f) * Mathf.PI) {
            // right
            inputFlags = INPUT_RIGHT;
        } else if (angle < (3f / 8f) * Mathf.PI) {
            // up-right
            inputFlags = INPUT_RIGHT | INPUT_UP;
        } else if (angle < (5f / 8f) * Mathf.PI) {
            // up
            inputFlags = INPUT_UP;
        } else if (angle < (7f / 8f) * Mathf.PI) {
            // up-left
            inputFlags = INPUT_LEFT | INPUT_UP;
        } else {
            // other left
            inputFlags = INPUT_LEFT;
        }
        */
        
        // probabilistic movement, deals better with corners
        float upness = Mathf.Max(move_vector.y, 0);
        float downness = Mathf.Max(-move_vector.y, 0);
        float leftness = Mathf.Max(-move_vector.x, 0);
        float rightness = Mathf.Max(move_vector.x, 0);
        inputFlags = 0;
        inputFlags |= Random.value <= upness ? INPUT_UP : 0;
        inputFlags |= Random.value <= downness ? INPUT_DOWN : 0;
        inputFlags |= Random.value <= leftness ? INPUT_LEFT : 0;
        inputFlags |= Random.value <= rightness ? INPUT_RIGHT : 0;
        
        
    }

    
    protected (float, Vector2, List<(Vector2, Vector2)>) ExploreFly(Vector2 pos, bool explore_inc, bool explore_dec, Collider2D ignore_collider, int depth, int max_depth)
    {
        Vector2 dest_delta = AIDestination - pos;
        Vector2 dest_direction = dest_delta.normalized;
        float best = dest_delta.magnitude;
        Vector2 move = dest_direction;
        List<(Vector2, Vector2)> path = new List<(Vector2, Vector2)>();
        path.Add((pos, move));
        
        if (depth > max_depth) return (best, move, path);
        
        void CheckIfBetter(float score, Vector2 action, List<(Vector2, Vector2)> trace)
        {
            if (score < best) {
                best = Mathf.Max(score, AICloseEnoughDistance);
                move = action;
                path = trace;
            }
        }
        
        RaycastHit2D direct = Physics2D.Raycast(pos, dest_delta, dest_delta.magnitude, AITerrainMask);
        float thickness = Mathf.Max(cl.bounds.extents.x, cl.bounds.extents.y);

        if (direct) {
            // there is something in the way
            Debug.DrawLine(pos, direct.point, Color.magenta, 0, false);
            DebugSquare(pos, Color.magenta);
            Vector2 new_pos = pos + (direct.distance - thickness) * dest_delta.normalized;
            best = DistanceToLineSegment(AIDestination, pos, new_pos);
            pos = new_pos;
            CompositeCollider2D level = null;
            if (direct.collider == ignore_collider) {
                ;
            } else {
                // duck typing
                if (direct.collider is CompositeCollider2D) {
                    level = (CompositeCollider2D) direct.collider;
                }

                if (level) {
                    // we hit the level geometry
                    best = DistanceToLineSegment(AIDestination, pos, direct.point);
                    List<List<Vector2>> coll_paths = new List<List<Vector2>>();
                    float closest_distance = float.MaxValue;
                    int closest_segment_path = 0;
                    int closest_segment_lower_point = 0;
                    for (int i = 0; i < level.pathCount; i++) {
                        List<Vector2> points = new List<Vector2>();
                        level.GetPath(i, points);
                        coll_paths.Add(points);
                        for (int p = 0; p < points.Count; p++) {
                            Vector2 lower_point = points[p];
                            Vector2 higher_point = points[MathMod(p + 1, points.Count)];
                            float dist = DistanceToLineSegment(direct.point, lower_point, higher_point);
                            if (dist < closest_distance) {
                                closest_distance = dist;
                                closest_segment_path = i;
                                closest_segment_lower_point = p;
                            }
                        }
                    }

                    (float, Vector2, List<(Vector2, Vector2)>) result_dec = (float.MaxValue, Vector2.zero, path);
                    (float, Vector2, List<(Vector2, Vector2)>) result_inc = (float.MaxValue, Vector2.zero, path);
                    if (explore_dec) {
                        result_dec = ExploreSurface(pos, direct.collider, coll_paths, closest_segment_path, closest_segment_lower_point, false, depth + 1, max_depth);
                    }

                    if (explore_inc) {
                        result_inc = ExploreSurface(pos, direct.collider, coll_paths, closest_segment_path, closest_segment_lower_point, true, depth + 1, max_depth);
                    }

                    Vector2 decide_action = dest_delta;
                    //if (direct.distance <= AIReachedWallDistance) {
                        CheckIfBetter(result_dec.Item1, result_dec.Item2, result_dec.Item3);
                        CheckIfBetter(result_inc.Item1, result_inc.Item2, result_inc.Item3);
                    //}

                }

                Vector2 wall_normal = direct.normal;
                Vector2 search_direction = Vector2.Perpendicular(wall_normal);
                List<(Vector2, Vector2)> new_path = new List<(Vector2, Vector2)>();
                new_path.Add((pos, move));
                CheckIfBetter(10000, dest_direction, new_path);
            }
        } else {
            // there are no obstructions! head straight for it
            Debug.DrawLine(pos, AIDestination, Color.green, 0, false);
            DebugSquare(pos, Color.green);
            List<(Vector2, Vector2)> new_path = new List<(Vector2, Vector2)>();
            new_path.Add((pos, move));
            new_path.Add((AIDestination, Vector2.zero));
            CheckIfBetter(0, dest_direction, new_path);
        }
        
        List<(Vector2, Vector2)> final_path = new List<(Vector2, Vector2)>();
        final_path.Add((pos, move));
        final_path.AddRange(path);
        return (best, move, final_path);
    }


    protected (float, Vector2, List<(Vector2, Vector2)>) ExploreSurface(Vector2 pos, Collider2D collider2D, List<List<Vector2>> paths, int current_path_idx, int lower_current_vertex, bool points_go_up, int depth, int max_depth)
    {
        Vector2 dest_delta = AIDestination - pos;
        float best = dest_delta.magnitude;
        Vector2 move = Vector2.zero;
        List<(Vector2, Vector2)> path = new List<(Vector2, Vector2)>();
        path.Add((pos, move));
        if (depth > max_depth) return (best, move, path);
        
        void CheckIfBetter(float score, Vector2 action, List<(Vector2, Vector2)> trace)
        {
            float tiny_depth_penalty = depth / 20f;
            if (score < best) {
                best = Mathf.Max(score, AICloseEnoughDistance + tiny_depth_penalty);
                move = action;
                path = trace;
            }
        }
        
        float thickness = Mathf.Max(cl.bounds.extents.x, cl.bounds.extents.y);
        List<Vector2> current_path = paths[current_path_idx];
        int path_length = current_path.Count;
        Vector2 current_start = current_path[MathMod(lower_current_vertex, path_length)];
        Vector2 current_end = current_path[MathMod(lower_current_vertex + 1, path_length)];
        DebugCircle(current_start, Color.blue);
        DebugCircle(current_end, Color.blue);
        Vector2 current_vector = current_end - current_start;
        int next_point_index;
        Vector2 next_vector;
        int target_point_index;
        if (points_go_up) {
            target_point_index = lower_current_vertex + 1;
            next_point_index = lower_current_vertex + 2;
            next_vector = current_path[MathMod(next_point_index, path_length)] -
                          current_path[MathMod(target_point_index, path_length)];
        } else {
            target_point_index = lower_current_vertex;
            next_point_index = lower_current_vertex - 1;
            next_vector = current_path[MathMod(lower_current_vertex, path_length)] -
                          current_path[MathMod(lower_current_vertex - 1, path_length)];
        }
        // remember to remove the * 0s !!! !!
        Vector2 current_normal = -Vector2.Perpendicular(current_vector).normalized;
        Vector2 next_normal = -Vector2.Perpendicular(next_vector).normalized;
        Vector2 target_pos_at_vertex = current_path[MathMod(target_point_index, path_length)] + (current_normal + next_normal) * thickness;
        DebugCircle(target_pos_at_vertex, Color.cyan);
        Vector2 displacement = target_pos_at_vertex - pos;
        best = DistanceToLineSegment(AIDestination, current_start, current_end);
        RaycastHit2D trace_edge = Physics2D.Raycast(pos, displacement, displacement.magnitude, AITerrainMask);
        Debug.DrawLine(pos, pos + displacement, Color.white);
        if (trace_edge) {
            // there was an obstruction to the next vertex
            Debug.DrawLine(pos, trace_edge.point, Color.red, 0, false);
            DebugDiamond(pos, Color.red);
            CheckIfBetter((AIDestination - trace_edge.point).magnitude, displacement.normalized, path);
        } else {
            // clear path to the corner
            Debug.DrawLine(pos, target_pos_at_vertex, Color.yellow, 0, false);
            DebugDiamond(pos, Color.yellow);
            if (best < AICloseEnoughDistance) {
                // we can reach the destination
                CheckIfBetter(best, displacement.normalized, path);
            } else {
                (float, Vector2, List<(Vector2, Vector2)>) result_fly = ExploreFly(target_pos_at_vertex, points_go_up, !points_go_up, collider2D, depth + 1, max_depth);
                (float, Vector2, List<(Vector2, Vector2)>) result_surface = ExploreSurface(target_pos_at_vertex, collider2D, paths, current_path_idx, points_go_up ? target_point_index : next_point_index, points_go_up, depth + 1, max_depth);
                CheckIfBetter(result_fly.Item1, displacement.normalized, result_fly.Item3);
                CheckIfBetter(result_surface.Item1, displacement.normalized, result_surface.Item3);
            }
        }
        
        List<(Vector2, Vector2)> final_path = new List<(Vector2, Vector2)>();
        final_path.Add((pos, move));
        final_path.AddRange(path);
        return (best, move, final_path);
    }
    
    
    protected bool ExploreShortcuts(Vector2 end_pos)
    {
        Vector2 start_pos = transform.position;
        Vector2 displacement = end_pos - start_pos;
        RaycastHit2D straight_shot = Physics2D.Raycast(start_pos, displacement, displacement.magnitude, AITerrainMask);
        if (straight_shot) {
            // nope, we can't make it
            Debug.DrawLine(start_pos, straight_shot.point, new Color(1, 0.5f, 0, 0.5f));
            return false;
        } else {
            // we can go here from the beginning!
            Debug.DrawLine(start_pos, end_pos, new Color(1, 0.5f, 0, 1.0f));
            return true;
        }
    }
    
    
    // thank you https://en.wikipedia.org/wiki/Vector_projection
    private float DistanceToLineSegment(Vector2 point, Vector2 start, Vector2 end)
    {
        Vector2 start_to_test = point - start;
        Vector2 line_segment = end - start;
        Vector2 normal = Vector2.Perpendicular(line_segment).normalized;
        Vector2 parallel = line_segment.normalized;
        float dx = Mathf.Cos(Vector2.SignedAngle(start_to_test, parallel) * Mathf.Deg2Rad) * start_to_test.magnitude;
        if (dx < 0) {
            return Vector2.Distance(point, start);
        } else if (dx > line_segment.magnitude) {
            return Vector2.Distance(point, end);
        } else {
            float dy = Mathf.Sin(Vector2.SignedAngle(start_to_test, parallel) * Mathf.Deg2Rad) * start_to_test.magnitude;
            return Mathf.Abs(dy);
        }
    }
    
    // this ALWAYS comes up and I don't understand why it's not in the language
    // https://stackoverflow.com/questions/2691025/mathematical-modulus-in-c-sharp
    static int MathMod(int a, int b) {
        return (Mathf.Abs(a * b) + a) % b;
    }

    public override void DoAbilityPrimaryDown(object sender, Vector3Args args)
    {
        if (!CanDoAbility()) return;
        if (startNewWind) {
            firstPoint = args.pos;
            startNewWind = false;
        } else {
            secondPoint = args.pos;
            // delete this and replace with correct things later!
            Instantiate(windSpawn, firstPoint, quaternion.identity);
            Instantiate(windSpawn, 0.75f * firstPoint + 0.25f * secondPoint, quaternion.identity);
            Instantiate(windSpawn, 0.5f * firstPoint + 0.5f * secondPoint, quaternion.identity);
            Instantiate(windSpawn, 0.25f * firstPoint + 0.75f * secondPoint, quaternion.identity);
            Instantiate(windSpawn, secondPoint, quaternion.identity);
            startNewWind = true;
            if (controlMode == controlModes.TopHalf) {
                GameEvents.InvokePlayerRegainFullControl();
            }
        }
    }

    public override void OnMoveToUp(object sender, Vector3Args args)
    {
        if (readyToMove && (controlMode == controlModes.BottomHalf || controlMode == controlModes.NPC)) {
            MoveToPoint(args.pos);
        }
    }
}
