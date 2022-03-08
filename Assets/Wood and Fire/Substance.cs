using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Substance : MonoBehaviour
{
    public string type;

    void Start()
    {
        
    }

    public string GetSubstanceType()
    {
        return type;
    }

    public virtual void SubstanceInteract(Substance otherSubstance){}

    void Update()
    {
        
    }
}
