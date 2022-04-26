using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenBorder : CameraConstraint
{

    [SerializeField] private float radius_of_influence = 30f;
    
    [SerializeField] private Vector2 size;

    [SerializeField] private bool left_border = false;
    [SerializeField] private bool right_border = false;
    [SerializeField] private bool up_border = false;
    [SerializeField] private bool down_border = false;

    [Tooltip("If false, the border is disabled when the player is behind it")]
    [SerializeField] private bool is_strong = true;

    public override (Vector3, float) ModifyCamera(Vector3 cam_target_pos, float cam_target_zoom, float vert_ext, float horiz_ext, DynamicCharacter follow_char, Vector3 follow_position, bool on_ground)
    {
        // this is a bit gross and I could have moved these to functions but I didn't
        Vector3 pos = transform.position;
        if ((cam_target_pos - pos).magnitude > radius_of_influence) return (cam_target_pos, cam_target_zoom);
        if (left_border) {
            // check if camera is in right zone
            if (cam_target_pos.y < pos.y + size.y / 2 && cam_target_pos.y > pos.y - size.y / 2 &&
                cam_target_pos.x - horiz_ext < pos.x + size.x / 2) {
                // check if player is in hidden zone
                if (is_strong || !(follow_position.y < pos.y + size.y / 2 && follow_position.y > pos.y - size.y / 2 && 
                                   follow_position.x < pos.x + size.x / 2)) {
                    cam_target_pos.x = pos.x + size.x / 2 + horiz_ext;
                }
            }
        }
        if (right_border) {
            if (cam_target_pos.y < pos.y + size.y / 2 && cam_target_pos.y > pos.y - size.y / 2 &&
                cam_target_pos.x + horiz_ext > pos.x - size.x / 2) {
                if (is_strong || !(follow_position.y < pos.y + size.y / 2 && follow_position.y > pos.y - size.y / 2 &&
                                   follow_position.x > pos.x - size.x / 2)) {
                    cam_target_pos.x = pos.x - size.x / 2 - horiz_ext;
                }
            }
        }
        if (down_border) {
            if (cam_target_pos.x < pos.x + size.x / 2 && cam_target_pos.x > pos.x - size.x / 2 &&
                cam_target_pos.y - vert_ext < pos.y + size.y / 2) {
                if (is_strong || !(follow_position.x < pos.x + size.x / 2 && follow_position.x > pos.x - size.x / 2 &&
                                   follow_position.y < pos.y + size.y / 2)) {
                    cam_target_pos.y = pos.y + size.y / 2 + vert_ext;
                }
            }
        }
        if (up_border) {
            if (cam_target_pos.x < pos.x + size.x / 2 && cam_target_pos.x > pos.x - size.x / 2 &&
                cam_target_pos.y + vert_ext > pos.y - size.y / 2) {
                if (is_strong || !(follow_position.x < pos.x + size.x / 2 && follow_position.x > pos.x - size.x / 2 &&
                                   follow_position.y > pos.y - size.y / 2)) {
                    cam_target_pos.y = pos.y - size.y / 2 - vert_ext;
                }
            }
        }
        return (cam_target_pos, cam_target_zoom);
    }

    public void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, size);
    }
}
