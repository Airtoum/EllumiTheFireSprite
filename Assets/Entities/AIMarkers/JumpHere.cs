using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpHere : AIMarker
{

    public Vector2 size = new Vector2(0.75f, 0.5f);
    public Vector2 jump_direction = new Vector2(4.5f, 14f);
    public Vector2 gravitational_acceleration = new Vector2(0, -30f);
    
    public const float JUMP_LINE_TIMESTEP = 0.1f;

    public override (bool, MarkerTypes, Vector2) ModifyAI(Vector2 pos)
    {
        return (IsWithinBox(pos, transform.position, size), MarkerTypes.JumpMarker, jump_direction);
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3 pos = transform.position;
        Gizmos.DrawWireCube(pos, size);
        Gizmos.DrawLine(LetterCoords(0.25f, 0.75f, pos, size), LetterCoords(0.75f, 0.75f, pos, size));
        Gizmos.DrawLine(LetterCoords(0.5f, 0.75f, pos, size), LetterCoords(0.5f, 0.25f, pos, size));
        Gizmos.DrawLine(LetterCoords(0.25f, 0.25f, pos, size), LetterCoords(0.5f, 0.25f, pos, size));
        float y_vel = jump_direction.y; 
        for (int i = 0; i <= 10; i++) {
            
            Vector3 next_pos = pos;
            next_pos.x += jump_direction.x * JUMP_LINE_TIMESTEP;
            next_pos.y += Ultramath.TrajectoryPos(y_vel, gravitational_acceleration.y, JUMP_LINE_TIMESTEP);
            y_vel = Ultramath.TrajectoryVel(y_vel, gravitational_acceleration.y, JUMP_LINE_TIMESTEP);
            Gizmos.DrawLine(pos, next_pos);
            pos = next_pos;
        }
    }

    private Vector3 LetterCoords(float x, float y, Vector3 pos, Vector3 box_size)
    {
        return (transform.position - (box_size / 2)) + new Vector3(x * box_size.x, y * box_size.y, 0f);
    }
    
    
}
