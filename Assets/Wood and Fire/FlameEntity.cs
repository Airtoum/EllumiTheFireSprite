using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlameEntity : Entity
{
    // Start is called before the first frame update
    [SerializeField] float lifeTimer = 2.5f;


    void Start()
    {
        
    }

    private void Update() 
    {
        lifeTimer -= Time.deltaTime;
        if (lifeTimer <= 0)
        {
            Destroy(this.gameObject);
        }
    }
}
