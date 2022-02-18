using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestEntity : MonoBehaviour
{

    [SerializeField] private float lifeLeft = 0f;

    // Update is called once per frame
    void Update()
    {
        lifeLeft -= Time.deltaTime;
        if (lifeLeft < 0) {
            Destroy(gameObject);
        } 
    }
}
