using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FollowCharacter : MonoBehaviour
{
    [SerializeField] public DynamicCharacter follow;
    [SerializeField] public Vector3 offset;

    [SerializeField] public float zoom;

    [SerializeField] public float scrollSpeed = 10f;
    [SerializeField] public float zoomSpeed = 10f;

    private Camera cam;
    
    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponent<Camera>();
        zoom = cam.orthographicSize;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        // thank you https://answers.unity.com/questions/230190/how-to-get-the-width-and-height-of-a-orthographic.html
        float camera_vertical_extent = cam.orthographicSize;
        float camera_height = 2 * camera_vertical_extent;
        float camera_width = camera_height * cam.aspect;
        float camera_horizontal_extent = camera_width / 2;
        
        Vector3 pos = transform.position;
        Vector3 target_pos = pos;
        Vector3 follow_pos = follow.transform.position;
        bool follow_on_ground = follow.GetOnGround();
        target_pos.x = follow_pos.x + offset.x;
        if (follow_on_ground) {
            target_pos.y = follow_pos.y + offset.y;
        }
        for (int i = 0; i < CameraConstraints.Constraints.Count; i++) {
            CameraConstraint constraint = CameraConstraints.Constraints[i];
            (Vector3, float) result = constraint.ModifyCamera(target_pos, zoom, camera_vertical_extent, camera_horizontal_extent, follow, follow_pos, follow_on_ground);
            target_pos = result.Item1;
            zoom = result.Item2;
        }
        transform.position = Vector3.MoveTowards(pos, target_pos, scrollSpeed * Time.deltaTime);
        cam.orthographicSize += Mathf.Clamp(zoom - camera_vertical_extent, -zoomSpeed * Time.deltaTime, zoomSpeed * Time.deltaTime);
    }
}
