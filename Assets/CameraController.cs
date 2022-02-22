using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{   
    [SerializeField] GameObject activeLevel;

    float xOffset;
    float yOffset;

    void Start()
    {
        //Sets the Offset Needed to Make Sure the Camera Does Not Move Past the Edges of a Level
        xOffset = this.gameObject.GetComponent<Camera>().rect.xMax / 2;
        yOffset = this.gameObject.GetComponent<Camera>().rect.yMax / 2;
    }

    void Update()
    {
        //Checks if a level is in play, and if it is, then clamps the coords to ensure the camera doesn't view past the edges of the level
        if (activeLevel != null && activeLevel.activeInHierarchy)
        {
            Mathf.Clamp(transform.position.x , activeLevel.GetComponent<LevelManager>().xCoordLow + xOffset, activeLevel.GetComponent<LevelManager>().xCoordHigh - xOffset);
            Mathf.Clamp(transform.position.y , activeLevel.GetComponent<LevelManager>().yCoordLow + yOffset, activeLevel.GetComponent<LevelManager>().yCoordHigh - yOffset);
        }
    }
}
