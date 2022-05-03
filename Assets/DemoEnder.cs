using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DemoEnder : MonoBehaviour
{

    [SerializeField] private string scene_name;
    
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnCollisionEnter2D(Collision2D other) 
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(scene_name);
    }
}
