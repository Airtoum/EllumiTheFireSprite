using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScrollText : MonoBehaviour
{
    float endPos = 540;
    [SerializeField] float startSpeed;

    private float currentSpeed;
    [SerializeField] float decreaseModifier;

    private float twoSecTimer = 2f;

    void Start()
    {
        currentSpeed = startSpeed;
    }

    void Update()
    {
        if (this.transform.position.y <= endPos)
        {
            currentSpeed = 0;
        }
        else
        {
            transform.position = new Vector3(transform.position.x, transform.position.y - (currentSpeed*Time.deltaTime), transform.position.z);
        }

        
        if (twoSecTimer <= 0)
        {
            twoSecTimer = 2f;
            updateSpeedModifier();
        }
        else
        {
            twoSecTimer -= Time.deltaTime;
        }
    }

    void updateSpeedModifier()
    {
        currentSpeed -= decreaseModifier;
        decreaseModifier *= 1.5f;
    }
}
