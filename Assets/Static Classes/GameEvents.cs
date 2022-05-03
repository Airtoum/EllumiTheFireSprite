using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vector3Args : EventArgs
{
    public Vector3 pos;
}

public class ObjectArgs : EventArgs
{
    public GameObject obj;
}

public class MainCharacterArgs : EventArgs
{
    public MainCharacter main_char;
}

public class DialogueArgs : EventArgs
{
    public MainCharacter main_char;
    public Dialogue dialogue;
}

public class TalkableArgs : EventArgs
{
    public Vector2 pos;
    public Talkable person;
}

public class GameEvents
{
    public static EventHandler<Vector3Args> OnPrimaryAbilityDown;
    public static EventHandler<Vector3Args> OnPrimaryAbilityHeld;
    public static EventHandler<Vector3Args> OnPrimaryAbilityUp;

    public static EventHandler<Vector3Args> OnMoveToDown;
    public static EventHandler<Vector3Args> OnMoveToUp;

    public static EventHandler PlayerRegainFullControl;
    public static EventHandler<ObjectArgs> UnpairPlayerControls;
    public static EventHandler<ObjectArgs> SelectPositionPlayerControls;

    public static EventHandler<DialogueArgs> StartDialogue;
    public static EventHandler EndDialogue;

    public static EventHandler<Vector3Args> ScanForTalkable;
    public static EventHandler<TalkableArgs> BroadcastTalkable;
    public static EventHandler<TalkableArgs> ATalkableHasBeenChosen;

    public static EventHandler KillWindZones;

    public static void InvokePrimaryAbilityDown(Vector3 position)
    {
        if (OnPrimaryAbilityDown == null) return;
        OnPrimaryAbilityDown(null, new Vector3Args{pos = position});
    }
    public static void InvokePrimaryAbilityHeld(Vector3 position)
    {
        if (OnPrimaryAbilityHeld == null) return;
        OnPrimaryAbilityHeld(null, new Vector3Args{pos = position});
    }
    public static void InvokePrimaryAbilityUp(Vector3 position)
    {
        if (OnPrimaryAbilityUp == null) return;
        OnPrimaryAbilityUp(null, new Vector3Args{pos = position});
    }
    public static void InvokeMoveToDown(Vector3 position)
    {
        if (OnMoveToDown == null) return;
        OnMoveToDown(null, new Vector3Args{pos = position});
    }
    public static void InvokeMoveToUp(Vector3 position)
    {
        if (OnMoveToUp == null) return;
        OnMoveToUp(null, new Vector3Args{pos = position});
    }
    public static void InvokePlayerRegainFullControl()
    {
        if (PlayerRegainFullControl == null) return;
        PlayerRegainFullControl(null, EventArgs.Empty);
    }
    public static void InvokeUnpairPlayerControls(GameObject helper)
    {
        if (UnpairPlayerControls == null) return;
        UnpairPlayerControls(null, new ObjectArgs{obj = helper});
    }
    public static void InvokeSelectPositionPlayerControls(GameObject helper)
    {
        if (SelectPositionPlayerControls == null) return;
        SelectPositionPlayerControls(null, new ObjectArgs{obj = helper});
    }
    public static void InvokeStartDialogue(MainCharacter talk_to, Dialogue the_dialogue)
    {
        if (StartDialogue == null) return;
        StartDialogue(null, new DialogueArgs {main_char = talk_to, dialogue = the_dialogue});
    }

    public static void InvokeEndDialogue()
    {
        if (EndDialogue == null) return;
        EndDialogue(null, EventArgs.Empty);
    }

    public static void InvokeScanForTalkable(Vector3 position)
    {
        if (ScanForTalkable == null) return;
        ScanForTalkable(null, new Vector3Args {pos = position});
    }
    
    public static void InvokeBroadcastTalkable(Vector2 position, Talkable talkable)
    {
        if (BroadcastTalkable == null) return;
        BroadcastTalkable(null, new TalkableArgs {pos = position, person = talkable});
    }

    public static void InvokeATalkableHasBeenChosen(Talkable talkable)
    {
        if (ATalkableHasBeenChosen == null) return;
        ATalkableHasBeenChosen(null, new TalkableArgs {pos = Vector2.zero, person = talkable});
    }
    
    public static void InvokeKillWindZones()
    {
        if (KillWindZones == null) return;
        KillWindZones(null, EventArgs.Empty);
    }
}