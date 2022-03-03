using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class AirSprite : MainCharacter
{

    [SerializeField] protected float AIReachedWallDistance = 1.2f; 
    
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
            return;
        }

        ExploreAir();
    }

    protected void ExploreAir()
    {
        (float, Vector2) result = ExploreFly(transform.position, true, true, null, 0, 20);
        Vector2 move_vector = result.Item2;
        float angle = -Vector2.SignedAngle(move_vector, Vector2.right) * Mathf.Deg2Rad;
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
    }

    
    protected (float, Vector2) ExploreFly(Vector2 pos, bool explore_inc, bool explore_dec, Collider2D ignore_collider, int depth, int max_depth)
    {
        Vector2 dest_delta = AIDestination - pos;
        if (depth > max_depth) return (dest_delta.magnitude, dest_delta);
        RaycastHit2D direct = Physics2D.Raycast(pos, dest_delta, dest_delta.magnitude, AITerrainMask);
        float thickness = Mathf.Max(cl.bounds.extents.x, cl.bounds.extents.y);
        float best;
        if (direct) {
            // there is something in the way
            Debug.DrawLine(pos, direct.point, Color.magenta, 0, false);
            DebugSquare(pos, Color.magenta);
            Vector2 new_pos = pos + (direct.distance - thickness) * dest_delta.normalized;
            best = DistanceToLineSegment(AIDestination, pos, new_pos);
            pos = new_pos;
            CompositeCollider2D level = null;
            if (direct.collider == ignore_collider) {
                return (best, dest_delta);
            }
            // duck typing
            if (direct.collider is CompositeCollider2D) {
                level = (CompositeCollider2D)direct.collider;
            }
            if (level) {
                // we hit the level geometry
                best = DistanceToLineSegment(AIDestination, pos, direct.point);
                List<List<Vector2>> paths = new List<List<Vector2>>();
                float closest_distance = float.MaxValue;
                int closest_segment_path = 0;
                int closest_segment_lower_point = 0;
                for(int i = 0; i < level.pathCount; i++) {
                    List<Vector2> points = new List<Vector2>();
                    level.GetPath(i, points);
                    paths.Add(points);
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
                (float, Vector2) result_dec = (float.MaxValue, Vector2.zero);
                (float, Vector2) result_inc = (float.MaxValue, Vector2.zero);
                if (explore_dec) {
                    result_dec = ExploreSurface(pos, direct.collider, paths, closest_segment_path,
                        closest_segment_lower_point, false, depth + 1, max_depth);
                }
                if (explore_inc) {
                    result_inc = ExploreSurface(pos, direct.collider, paths, closest_segment_path,
                        closest_segment_lower_point, true, depth + 1, max_depth);
                }
                Vector2 decide_action = dest_delta; 
                if (direct.distance <= AIReachedWallDistance) {
                    if (result_dec.Item1 < best) decide_action = result_dec.Item2;
                    best = Mathf.Min(best, result_dec.Item1);
                    if (result_inc.Item1 < best) decide_action = result_inc.Item2;
                    best = Mathf.Min(best, result_inc.Item1);
                }
                best = Mathf.Min(best, result_dec.Item1, result_inc.Item1);
                return (best, decide_action);
            }
            Vector2 wall_normal = direct.normal;
            Vector2 search_direction = Vector2.Perpendicular(wall_normal);
            return (10000, dest_delta);
        } else {
            // there are no obstructions! head straight for it
            Debug.DrawLine(pos, AIDestination, Color.green, 0, false);
            DebugSquare(pos, Color.green);
            best = 0;
            return (best, dest_delta);
        }
    }

    
    protected (float, Vector2) ExploreSurface(Vector2 pos, Collider2D collider2D, List<List<Vector2>> paths, int current_path_idx, int lower_current_vertex, bool points_go_up, int depth, int max_depth)
    {
        Vector2 dest_delta = AIDestination - pos;
        if (depth > max_depth) return (dest_delta.magnitude, dest_delta);
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
        float best = DistanceToLineSegment(AIDestination, current_start, current_end);
        RaycastHit2D trace_edge = Physics2D.Raycast(pos, displacement, displacement.magnitude, AITerrainMask);
        Debug.DrawLine(pos, pos + displacement, Color.white);
        if (trace_edge) {
            // there was an obstruction to the next vertex
            Debug.DrawLine(pos, trace_edge.point, Color.red, 0, false);
            DebugDiamond(pos, Color.red);
            return ((AIDestination - trace_edge.point).magnitude, displacement);
        } else {
            // clear path to the corner
            Debug.DrawLine(pos, target_pos_at_vertex, Color.yellow, 0, false);
            DebugDiamond(pos, Color.yellow);
            if (best < AICloseEnoughDistance) {
                // we can reach the destination
                return (best, displacement);
            }
            (float, Vector2) result_fly = ExploreFly(target_pos_at_vertex, points_go_up, !points_go_up, collider2D, depth + 1, max_depth);
            (float, Vector2) result_surface = ExploreSurface(target_pos_at_vertex, collider2D, paths, current_path_idx,
                points_go_up ? target_point_index : next_point_index, points_go_up, depth + 1, max_depth);
            if (result_fly.Item1 < result_surface.Item1) {
                // we can fly there from the corner
                return (result_fly.Item1, displacement);
            } else {
                // we can get there by continuing to examine the surface
                return (result_surface.Item1, displacement);
            }
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
    
    public override void OnMoveToUp(object sender, Vector3Args args)
    {
        if (readyToMove && (controlMode == controlModes.TopHalf || controlMode == controlModes.NPC)) {
            MoveToPoint(args.pos);
        }
    }
}
