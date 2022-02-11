using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Can be overridden to do more on death
    public void Die()
    {
        Destroy(gameObject);
    }
}
