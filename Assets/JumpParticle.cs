using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpParticle : MonoBehaviour
{

    private float lifeTime = .15f;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (lifeTime <= 0)
        {
            Destroy(this.gameObject);
        }
        else
        {
            lifeTime -= Time.deltaTime;
        }
    }
}
