using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wood : Substance
{
    [SerializeField] GameObject emberGenerator;

    [SerializeField] GameObject nonCollidingEmberGenerator;

    [SerializeField] Material spriteRegular;
    [SerializeField] Material burnShader;
    [SerializeField] Material instanceBurnShader;

    float amountDisappeared = 1f;

    bool isBurning = false;

    private void Start() 
    {

    }

    private void Update() 
    {
        if (isBurning)
        {
            amountDisappeared -= Time.deltaTime * 0.4f;
            if (amountDisappeared <= 0f)
            {
                amountDisappeared = 0f;
                isBurning = false;
            }
            instanceBurnShader.SetFloat("_Fade", amountDisappeared);
        }
    }

    private void Awake() 
    {
        type = "Wood";
        this.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
        emberGenerator.SetActive(false);
        nonCollidingEmberGenerator.SetActive(false);
        instanceBurnShader = Material.Instantiate(burnShader);
    }
    
    private void OnCollisionEnter2D(Collision2D other) 
    {
        Debug.Log("Collision Detected");
        if (other.gameObject.GetComponent<Substance>() != null)
        { 
            Substance triggerSubstance = other.gameObject.GetComponent<Substance>();
            SubstanceInteract(triggerSubstance);
        }
    }

    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.gameObject.GetComponent<Substance>() != null)
        { 
            Substance triggerSubstance = other.gameObject.GetComponent<Substance>();
            SubstanceInteract(triggerSubstance);
        }
    }


    private void OnParticleCollision(GameObject other) 
    {
        if (other.gameObject.GetComponent<Substance>() != null)
        { 
            Substance triggerSubstance = other.gameObject.GetComponent<Substance>();
            float burnProbability = Random.Range(0, 100);
            
            if (burnProbability < 40)
            {
                SubstanceInteract(triggerSubstance);
            }
        }
    }

    public override void SubstanceInteract(Substance triggerSubstance)
    {
        if (triggerSubstance.GetSubstanceType() == "Flame")
        {
            emberGenerator.SetActive(true);
            nonCollidingEmberGenerator.SetActive(true);
            GetComponent<SpriteRenderer>().material = instanceBurnShader;
            isBurning = true;
            this.gameObject.GetComponent<SpriteRenderer>().color = Color.yellow;
            Destroy(this.gameObject, 3);
        }
        else
        {
            return;
        }
    }
}
