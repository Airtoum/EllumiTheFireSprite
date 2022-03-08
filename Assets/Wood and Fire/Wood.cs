using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wood : Substance
{
    [SerializeField] GameObject emberGenerator;
    [SerializeField] Material spriteRegular;

    [SerializeField] Material burnShader;

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
            burnShader.SetFloat("_Fade", amountDisappeared);
        }
    }

    private void Awake() 
    {
        type = "Wood";
        this.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
        emberGenerator.SetActive(false);
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

    public override void SubstanceInteract(Substance triggerSubstance)
    {
        if (triggerSubstance.GetSubstanceType() == "Flame")
        {
            emberGenerator.SetActive(true);
            GetComponent<SpriteRenderer>().material = burnShader;
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
