using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

public class AirSprite : MainCharacter
{
    
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
        ExploreFly();
    }

    protected (float, Vector2) ExploreFly(Vector2 pos,  int depth, int max_depth)
    {
        Vector2 dest_delta = AIDestination - pos;
        RaycastHit2D direct = Physics2D.Raycast(pos, dest_delta, dest_delta.magnitude, AITerrainMask);
        float thickness = Mathf.Max(cl.bounds.extents.x, cl.bounds.extents.y);
        if (direct) {
            // there is something in the way
            pos = (direct.distance - thickness) * dest_delta.normalized;
            CompositeCollider2D level = direct.collider.composite;
            if (level) {
                // we hit the level geometry
                for(int i = 0; i < level.pathCount; i++) {
                    List<Vector2> points = new List<Vector2>();
                    level.GetPath(i, points);
                    
                }
            }
            Vector2 wall_normal = direct.normal;
            Vector2 search_direction = Vector2.Perpendicular(wall_normal);
            
        } else {
            // there are no obstructions! head straight for it
            return (0, dest_delta);
        }
    }

    protected (float, Vector2) ExploreSurface(Vector2 pos, Vector2 direction, Vector2 towards_wall, int depth, int max_depth)
    {
        float thickness = Mathf.Max(cl.bounds.extents.x, cl.bounds.extents.y);
        RaycastHit2D forward = Physics2D.Raycast(pos, direction, thickness, AITerrainMask);
        if (forward) {
            // we hit something
        } else {
            // we didn't hit something
            RaycastHit2D wall_check = Physics2D.Raycast(pos, towards_wall, 4)
        }
    }

    // it doesn't have to be signed but it's easier to keep it signed
    // thank you https://en.wikipedia.org/wiki/Vector_projection
    private float SignedDistanceToLine(Vector2 point, Vector2 start, Vector2 end)
    {
        Vector2 start_to_test = point - start;
        Vector2 line_segment = end - start;
        Vector2 normal = Vector2.Perpendicular(line_segment).normalized;
        Vector2 parallel = line_segment.normalized;
        Vector2 projected_onto_line = UnityEngine.Vector3.Project()
    }
}
