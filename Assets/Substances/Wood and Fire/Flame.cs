using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flame : Substance 
{
    private void Awake() 
    {
        type = "Flame";
    }

    private void OnCollisionEnter2D(Collision2D other) 
    {
        if (other.gameObject.GetComponent<Substance>() != null)
        { 
            Substance triggerSubstance = other.gameObject.GetComponent<Substance>();
            if (triggerSubstance.GetSubstanceType() != "Flame")
            { 
                SubstanceInteract(triggerSubstance);
            }
        }
    }

    public override void SubstanceInteract(Substance triggerSubstance)
    {
        if (triggerSubstance.GetSubstanceType() == "Water")
        {
            Destroy(this.gameObject, 1);
        }
    }
}
