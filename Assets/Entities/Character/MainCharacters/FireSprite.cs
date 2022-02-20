using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class FireSprite : MainCharacter
{

    public Entity flameSpawn;
    public Transform flameSpawnFrom;
    public float flameSpeed;

    public float flameInterval = 0.08f;
    public float flameTimer = 0;

    public override void DoAbilityPrimaryDown(Vector3 position)
    {
        flameTimer = flameInterval;
    }
    
    public override void DoAbilityPrimaryHold(Vector3 position)
    {
        flameTimer += Time.deltaTime;
        if (flameTimer < flameInterval) return;
        
        Vector3 origin = flameSpawnFrom.position;
        Vector2 direction = position - origin;
        direction = direction.normalized * flameSpeed;
        Entity newFlame = Instantiate(flameSpawn, origin, Quaternion.identity);
        // this is gross
        newFlame.SetVelocity(direction);
        flameTimer = 0;
    }
    
}
