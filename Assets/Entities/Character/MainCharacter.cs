using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCharacter : DynamicCharacter
{

    public enum controlModes
    {
        PlayerControl,
        NPC,
        TopHalf,
        BottomHalf
    }

    public controlModes controlMode;
    
    // Start is called before the first frame update
    protected new void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected new void FixedUpdate()
    {
        if (controlMode == controlModes.PlayerControl || controlMode == controlModes.BottomHalf) {
            inputFlags = 0;
            // C# can't convert ints to bools or bools to ints
            inputFlags |= (Input.GetAxis("Left")>0) ? INPUT_LEFT : 0;
            inputFlags |= (Input.GetAxis("Right")>0) ? INPUT_RIGHT : 0;
            inputFlags |= (Input.GetAxis("Down")>0) ? INPUT_DOWN : 0;
            inputFlags |= (Input.GetAxis("Up")>0) ? INPUT_UP : 0;
            inputFlags |= (Input.GetAxis("Jump")>0) ? INPUT_JUMP : 0;
            inputFlags |= (Input.GetAxis("Fire1")>0) ? INPUT_SPECIAL : 0;
        }
        base.FixedUpdate();
    }

    public new virtual void DoAbilityPrimaryDown(object sender, Vector3Args args)
    {
        // override with cool stuff
    }
    
    public new virtual void DoAbilityPrimaryHold(object sender, Vector3Args args)
    {
        // override with cool stuff
    }
    
    public new virtual void DoAbilityPrimaryUp(object sender, Vector3Args args)
    {
        // override with cool stuff
    }
    
    public new virtual void OnMoveToDown(object sender, Vector3Args args)
    {
        // override with cool stuff
    }
    public new virtual void OnMoveToUp(object sender, Vector3Args args)
    {
        // override with cool stuff
    }
}
