using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireSprite : MainCharacter
{

    public GameObject flameSpawn;
    
    public override void DoAbilityPrimary(Vector3 position)
    {
        Instantiate(flameSpawn, position, Quaternion.identity);
    }
    
}
