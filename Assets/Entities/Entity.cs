using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{

    private Rigidbody2D rb_nullable;
    [NonSerialized] public Vector2 faux_velocity;
    [SerializeField, Tooltip("Leave this false if you want to use Rigidbody2D. Set to true to hide the warning when using SetVelocity.")] 
    public bool usesFauxVelocity; // this turns off the warning
    
    // Start is called before the first frame update
    void Awake()
    {
        if (TryGetComponent(out Rigidbody2D rb_check)) {
            rb_nullable = rb_check;
        }
    }

    private void FixedUpdate()
    {
        transform.position += (Vector3)faux_velocity * Time.fixedDeltaTime;
    }

    // Can be overridden to do more on death
    public void Die()
    {
        Destroy(gameObject);
    }

    // override with things you do per-collison
    protected void EvaluateCollision(Collision2D coll, ContactPoint2D contact)
    {
        
    }

    // I think this is also gross
    public void SetVelocity(Vector2 vel)
    {
        if (rb_nullable) {
            rb_nullable.velocity = vel;
        } else {
            if (!usesFauxVelocity) {Debug.LogWarning(gameObject.name + " is using faux velocity.");}
            faux_velocity = vel;
        }
    }
}
