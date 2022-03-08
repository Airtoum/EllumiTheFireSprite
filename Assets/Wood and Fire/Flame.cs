using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flame : Substance 
{
    [SerializeField] GameObject player;

    private void Awake() 
    {
        type = "Flame";
    }

    private void OnCollisionEnter2D(Collision2D other) 
    {
        if (other.gameObject.GetComponent<Substance>() != null && other.gameObject.layer == 7)
        { 
            Substance triggerSubstance = other.gameObject.GetComponent<Substance>();
            SubstanceInteract(triggerSubstance);
        }
        else
        {
            Physics2D.IgnoreCollision(other.collider, other.otherCollider);
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
