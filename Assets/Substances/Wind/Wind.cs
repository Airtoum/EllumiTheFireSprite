using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wind : Substance
{
    [SerializeField] float totalForceMultiplier;
    [SerializeField] GameObject areaOfEffect;

    [SerializeField] float WindZoneWidth;

    [SerializeField] GameObject backPoint;
    [SerializeField] GameObject frontPoint;
    
    Vector2 forceAdded = new Vector2();
    
    [SerializeField] float ForceMagnitude;

    void Start()
    {
        this.type = "Wind";
        
        CalculateForceDirection();
    }

    void Update()
    {
        CalculateForceDirection();
    }


    void CalculateForceDirection()
    {
        // Vector2 Direction = new Vector2(areaOfEffect.GetComponent<Renderer>().bounds.max.x, areaOfEffect.GetComponent<Renderer>().bounds.max.y) - new Vector2(areaOfEffect.GetComponent<Renderer>().bounds.min.x, areaOfEffect.GetComponent<Renderer>().bounds.max.y); 
        
        Vector2 Direction = new Vector2();

        Vector3 pseudoDirection =frontPoint.transform.position - backPoint.transform.position;
        Direction = new Vector2(pseudoDirection.x, pseudoDirection.y);
        
        Direction.Normalize();


        forceAdded = Direction * totalForceMultiplier;

        ForceMagnitude = forceAdded.magnitude;

        Debug.DrawRay(areaOfEffect.GetComponent<Renderer>().bounds.center, forceAdded);
        
        
        Vector2 windZoneSize = new Vector2(frontPoint.transform.localPosition.x - backPoint.transform.localPosition.x, WindZoneWidth);
        this.GetComponent<BoxCollider2D>().size = windZoneSize;
        this.GetComponentInChildren<SpriteRenderer>().transform.localScale = new Vector3(windZoneSize.x, windZoneSize.y,this.GetComponentInChildren<SpriteRenderer>().transform.localScale.z);
    }

    private void OnTriggerStay2D(Collider2D other) 
    {
        if (other.gameObject.GetComponent<Substance>() != null)
        { 
            Substance triggerSubstance = other.gameObject.GetComponent<Substance>();
            SubstanceInteract(triggerSubstance);
        }
    }
    public override void SubstanceInteract(Substance triggerSubstance)
    {
        if (triggerSubstance.gameObject.GetComponent<Rigidbody2D>() != null)
        {
            triggerSubstance.gameObject.GetComponent<Rigidbody2D>().AddForce(forceAdded);
        }
    }
}
