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
    public bool readyToMove = false;
    public bool isInCutscene = false;
    private bool cancelAfterDialogueJump = false;

    public bool isFollowing = true;
    public Transform helperFollow = null;
    public Vector2 followOffset = Vector2.zero;
    public float tooFarAwayDistance = 14f;

    protected void Awake()
    {
        GameEvents.PlayerRegainFullControl += OnPlayerRegainFullControl;
        GameEvents.UnpairPlayerControls += OnUnpairPlayerControls;
        GameEvents.SelectPositionPlayerControls += OnSelectPositionPlayerControls;

        GameEvents.OnMoveToDown += OnMoveToDown;
        GameEvents.OnMoveToUp += OnMoveToUp;

        GameEvents.StartDialogue += OnStartDialogue;
        GameEvents.EndDialogue += OnEndDialogue;
    }
    
    // Start is called before the first frame update
    protected new void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    protected new void FixedUpdate()
    {
        if (cancelAfterDialogueJump) {
            if (Input.GetAxis("Jump") <= 0) cancelAfterDialogueJump = false;
        }
        if (!isInCutscene && (controlMode == controlModes.PlayerControl || controlMode == controlModes.BottomHalf)) {
            inputFlags = 0;
            // C# can't convert ints to bools or bools to ints
            inputFlags |= (Input.GetAxis("Left")>0) ? INPUT_LEFT : 0;
            inputFlags |= (Input.GetAxis("Right")>0) ? INPUT_RIGHT : 0;
            inputFlags |= (Input.GetAxis("Down")>0) ? INPUT_DOWN : 0;
            inputFlags |= (Input.GetAxis("Up")>0) ? INPUT_UP : 0;
            inputFlags |= (Input.GetAxis("Jump")>0 && !cancelAfterDialogueJump) ? INPUT_JUMP : 0;
            inputFlags |= (Input.GetAxis("Fire1")>0) ? INPUT_SPECIAL : 0;
        }
        if (!isInCutscene && helperFollow && (controlMode == controlModes.TopHalf || controlMode == controlModes.NPC)) {
            if ((helperFollow.position - transform.position).magnitude > tooFarAwayDistance) {
                isFollowing = true;
            }
            if (isFollowing) {
                MoveToPoint((Vector2)helperFollow.position + followOffset);
            }
        }
        base.FixedUpdate();
    }

    private void OnPlayerRegainFullControl(object sender, EventArgs args)
    {
        if (controlMode == controlModes.BottomHalf) {
            controlMode = controlModes.PlayerControl;
        }
        if (controlMode == controlModes.TopHalf) {
            controlMode = controlModes.NPC;
        }
    }

    private void OnUnpairPlayerControls(object sender, ObjectArgs args)
    {
        if (controlMode == controlModes.PlayerControl) {
            controlMode = controlModes.BottomHalf;
        }

        if (controlMode == controlModes.NPC && gameObject == args.obj) {
            controlMode = controlModes.TopHalf;
        }
    }

    private void OnSelectPositionPlayerControls(object sender, ObjectArgs args)
    {
        if (controlMode == controlModes.PlayerControl) {
            controlMode = controlModes.BottomHalf;
        }
        if (controlMode == controlModes.NPC && gameObject == args.obj) {
            readyToMove = true;
        }
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
        if (readyToMove && (controlMode == controlModes.BottomHalf || controlMode == controlModes.NPC)) {
            MoveToPoint(args.pos);
            isFollowing = false;
        }
    }

    public void OnStartDialogue(object sender, DialogueArgs args)
    {
        this.isInCutscene = true;
        inputFlags = 0;
    }
    
    public void OnEndDialogue(object sender, EventArgs args)
    {
        this.isInCutscene = false;
        cancelAfterDialogueJump = true;
    }

    public bool CanDoAbility()
    {
        return (controlMode == controlModes.TopHalf || controlMode == controlModes.PlayerControl);
    }
}
