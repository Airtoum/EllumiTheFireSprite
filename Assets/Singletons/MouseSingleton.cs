using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseSingleton : MonoBehaviour
{
    public static MouseSingleton instance;
    public MainCharacter currentCharacter;
    public bool isInCutscene = false;
    
    public enum MouseModes
    {
        None,
        Ability,
        Move
    }

    public MouseModes CurrentMouseMode = MouseModes.Ability;
    
    // Start is called before the first frame update
    private void Awake()
    {
        MouseSingleton.instance = this;
        GameEvents.UnpairPlayerControls += OnUnpairPlayerControls;
        GameEvents.SelectPositionPlayerControls += OnSelectPositionPlayerControls;
        GameEvents.PlayerRegainFullControl += OnPlayerRegainFullControl;

        GameEvents.StartDialogue += OnStartDialogue;
        GameEvents.EndDialogue += OnEndDialogue;
    }

    void OnDestroy()
    {
        GameEvents.UnpairPlayerControls -= OnUnpairPlayerControls;
        GameEvents.SelectPositionPlayerControls -= OnSelectPositionPlayerControls;
        GameEvents.PlayerRegainFullControl -= OnPlayerRegainFullControl;

        GameEvents.StartDialogue -= OnStartDialogue;
        GameEvents.EndDialogue -= OnEndDialogue;
    }

    // Update is called once per frame
    void Update()
    {
        if (isInCutscene) return;
        
        // if LMB
        if (Input.GetMouseButtonDown(0)) {
            Vector3 clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            clickPosition.z = 0;
            if (CurrentMouseMode == MouseModes.Ability) {
                GameEvents.InvokePrimaryAbilityDown(clickPosition);
            }
            if (CurrentMouseMode == MouseModes.Move) {
                GameEvents.InvokeMoveToDown(clickPosition);
            }
        }
        // if it's being held down
        if (Input.GetMouseButton(0)) {
            Vector3 clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            clickPosition.z = 0;
            if (CurrentMouseMode == MouseModes.Ability) {
                GameEvents.InvokePrimaryAbilityHeld(clickPosition);
            }
            if (CurrentMouseMode == MouseModes.Move) {
                // maybe questionable
                GameEvents.InvokeMoveToDown(clickPosition);
            }
        }
        // on release
        if (Input.GetMouseButtonUp(0)) {
            Vector3 clickPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            clickPosition.z = 0;
            if (CurrentMouseMode == MouseModes.Ability) {
                GameEvents.InvokePrimaryAbilityUp(clickPosition);
            }
            if (CurrentMouseMode == MouseModes.Move) {
                GameEvents.InvokeMoveToUp(clickPosition);
            }
        }
    }

    
    // keeping track of currentCharacter is vestigial
    void OnUnpairPlayerControls(object sender, ObjectArgs args)
    {
        MainCharacter main_char = args.obj.GetComponent<MainCharacter>();
        if (main_char) {
            currentCharacter = main_char;
            CurrentMouseMode = MouseModes.Ability;
        } else {
            Debug.LogWarning(args.obj.name + " is not a MainCharacter, but is being asked to redistribute the controls.");
        }
    }
    
    void OnSelectPositionPlayerControls(object sender, ObjectArgs args)
    {
        MainCharacter main_char = args.obj.GetComponent<MainCharacter>();
        if (main_char) {
            currentCharacter = main_char;
            CurrentMouseMode = MouseModes.Move;
        } else {
            Debug.LogWarning(args.obj.name + " is not a MainCharacter, but is being asked to redistribute the controls.");
        }
    }
    
    void OnPlayerRegainFullControl(object sender, EventArgs args)
    {
        CurrentMouseMode = MouseModes.Ability;
    }

    void OnStartDialogue(object sender, DialogueArgs args)
    {
        isInCutscene = true;
    }

    void OnEndDialogue(object sender, EventArgs args)
    {
        isInCutscene = false;
    }
    
}
