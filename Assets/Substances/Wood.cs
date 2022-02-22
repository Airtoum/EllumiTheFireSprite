using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wood : Substance
{
    private void Awake() 
    {
        type = "Wood";
    }
    
    private void OnCollisionEnter2D(Collision2D other) 
    {
        Debug.Log("Collision Detected");
        Substance triggerSubstance = other.gameObject.GetComponent<Substance>();
        SubstanceInteract(triggerSubstance);
    }

    public override void SubstanceInteract(Substance triggerSubstance)
    {
        if (triggerSubstance.GetSubstanceType() == "Flame")
        {
            Destroy(this.gameObject, 3);
        }
        else
        {
            return;
        }
    }
}
