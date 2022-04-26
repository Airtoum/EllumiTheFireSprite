using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundAnchor : MonoBehaviour
{
    bool isColliding = false;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public bool isAnchored()
    {
        return isColliding;
    }

    private void OnCollisionEnter2D(Collision2D other) 
    {
        isColliding = true;
    }

    private void OnCollisionExit2D(Collision2D other) 
    {
        isColliding = false;
    }
}
