using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseSingleton : MonoBehaviour
{
    public static MouseSingleton instance;
    public MainCharacter currentCharacter; 
    
    // Start is called before the first frame update
    private void Awake()
    {
        MouseSingleton.instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        // if LMB
        if (Input.GetMouseButtonDown(0)) {
            print("click!");
            // clean this up later
            Vector3 clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            clickPosition.z = 0;
            //currentCharacter.DoAbilityPrimaryDown(clickPosition);
            currentCharacter.MoveToPoint(clickPosition);
        }
        // if it's being held down
        if (Input.GetMouseButton(0)) {
            Vector3 clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            clickPosition.z = 0;
            //currentCharacter.DoAbilityPrimaryHold(clickPosition);
        }
    }
    
}
