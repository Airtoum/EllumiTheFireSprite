using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIMarker : MonoBehaviour
{
    
    public enum MarkerTypes{
        None, JumpMarker
    }
    
    void Awake()
    {
        AIMarkers.Markers.Add(this);
    }

    private void OnDestroy()
    {
        AIMarkers.Markers.Remove(this);
    }

    public virtual (bool, MarkerTypes, Vector2) ModifyAI(Vector2 pos)
    {
        return (false, MarkerTypes.None, Vector2.zero);
    }
    
    public static bool IsWithinBox(Vector2 point, Vector2 box_center, Vector2 box_size)
    {
        return (point.y < box_center.y + box_size.y / 2 &&
                point.y > box_center.y - box_size.y / 2 &&
                point.x < box_center.x + box_size.x / 2 &&
                point.x > box_center.x - box_size.x / 2);
    }
}
