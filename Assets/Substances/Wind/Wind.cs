using System;
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

    private BoxCollider2D box_collider;

    [SerializeField] public float lifetime;
    private float age = 0;

    [SerializeField] public bool natural = false; 


    private void Awake()
    {
        GameEvents.KillWindZones += OnKillWindZones;
    }

    void Start()
    {
        this.type = "Wind";
        box_collider = this.GetComponent<BoxCollider2D>();
        
        CalculateForceDirection();
    }

    void Update()
    {
        CalculateForceDirection();
        age += Time.deltaTime;
        if (age > lifetime) {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        GameEvents.KillWindZones -= OnKillWindZones;
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
        box_collider.size = windZoneSize;
        box_collider.offset = new Vector2(windZoneSize.x / 2, box_collider.offset.y);
        GameObject animatedWindZone = this.GetComponentInChildren<Animator>().gameObject;
        animatedWindZone.GetComponent<SpriteRenderer>().size = windZoneSize;
        areaOfEffect.transform.localPosition = new Vector3(windZoneSize.x/2, 0, 0);
        areaOfEffect.transform.localScale = windZoneSize;
        animatedWindZone.transform.localPosition = new Vector3(windZoneSize.x/2, 0, 0);
        //animatedWindZone.transform.localScale = windZoneSize;
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

    public void OnKillWindZones(object sender, EventArgs args)
    {
        if (!natural) {
            Destroy(gameObject);
        }
    }
}