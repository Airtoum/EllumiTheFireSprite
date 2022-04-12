using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ZoomArea : CameraConstraint
{

    [SerializeField] private Vector2 size = new Vector2(1, 20);
    [SerializeField] public float zoom = 5;

    [SerializeField] private bool if_player_within = false;
    [SerializeField] private bool if_camera_within = true;
    
    public override (Vector3, float) ModifyCamera(Vector3 cam_target_pos, float cam_target_zoom, float vert_ext, float horiz_ext, DynamicCharacter follow_char, Vector3 follow_position, bool on_ground)
    {
        // this is a bit gross and I could have moved these to functions but I didn't
        Vector3 pos = transform.position;
        if (if_player_within) {
            if (CameraConstraint.IsWithinBox(follow_position, pos, size)) {
                cam_target_zoom = zoom;
            }
        }
        if (if_camera_within) {
            if (CameraConstraint.IsWithinBox(cam_target_pos, pos, size)) {
                cam_target_zoom = zoom;
            }
        }
        return (cam_target_pos, cam_target_zoom);
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = new Color(1.0f, 0.6f, 0.6f);
        Gizmos.DrawWireCube(transform.position, size);
    }
}
