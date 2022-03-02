using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vector3Args : EventArgs
{
    public Vector3 pos;
}

public class GameEvents
{
    public static EventHandler<Vector3Args> OnPrimaryAbilityDown;
    public static EventHandler<Vector3Args> OnPrimaryAbilityHeld;
    public static EventHandler<Vector3Args> OnPrimaryAbilityUp;

    public static EventHandler OnMoveToDown;

    public static void InvokePrimaryAbilityDown(Vector3 position)
    {
        OnPrimaryAbilityDown(null, new Vector3Args{pos = position});
    }
    public static void InvokePrimaryAbilityHeld(Vector3 position)
    {
        OnPrimaryAbilityHeld(null, new Vector3Args{pos = position});
    }
    public static void InvokePrimaryAbilityUp(Vector3 position)
    {
        OnPrimaryAbilityUp(null, new Vector3Args{pos = position});
    }
}