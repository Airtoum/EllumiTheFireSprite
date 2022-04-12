using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class CameraConstraint : MonoBehaviour
{
    void Awake()
    {
        CameraConstraints.Constraints.Add(this);
    }

    private void OnDestroy()
    {
        CameraConstraints.Constraints.Remove(this);
    }

    // returns adjusted target position and target zoom levels
    public virtual (Vector3, float) ModifyCamera(Vector3 cam_target_pos, float cam_target_zoom, float vert_ext, float horiz_ext, DynamicCharacter follow_char, Vector3 follow_position, bool on_ground)
    {
        // adjust the camera here
        return (cam_target_pos, cam_target_zoom);
    }

    public static bool IsWithinBox(Vector2 point, Vector2 box_center, Vector2 box_size)
    {
        return (point.y < box_center.y + box_size.y / 2 &&
                point.y > box_center.y - box_size.y / 2 &&
                point.x < box_center.x + box_size.x / 2 &&
                point.x > box_center.x - box_size.x / 2);
    }
}
