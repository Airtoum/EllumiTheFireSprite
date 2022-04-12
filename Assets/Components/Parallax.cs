using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{

    [SerializeField, Range(0, 1)] public float parallax;
    private Vector3 anchor;
    
    // Start is called before the first frame update
    void Start()
    {
        anchor = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = (parallax) * anchor + (1 - parallax) * Camera.main.transform.position;
    }
}
