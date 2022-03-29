using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ultramath : MonoBehaviour
{
    // this ALWAYS comes up and I don't understand why it's not in the language
    // https://stackoverflow.com/questions/2691025/mathematical-modulus-in-c-sharp
    public static int MathMod(int a, int b) {
        return (Mathf.Abs(a * b) + a) % b;
    }

    // wrote these myself though
    public static float TrajectoryPos(float start_vel, float accel, float x)
    {
        return start_vel * x + 0.5f * accel * x * x;
    }

    public static float TrajectoryVel(float start_vel, float accel, float x)
    {
        return start_vel + accel * x;
    }
    
    // thank you https://en.wikipedia.org/wiki/Vector_projection
    public static float DistanceToLineSegment(Vector2 point, Vector2 start, Vector2 end)
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
}
