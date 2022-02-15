using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class CollisionData : MonoBehaviour
{

    public List<Collision2D> collisions = new List<Collision2D>();

    private void Start()
    {
    }

    private void FixedUpdate()
    {
        collisions.Clear();
    }

    public void OnCollisionEnter2D(Collision2D other)
    {
        collisions.Add(other);
    }
    
    public void OnCollisionExit2D(Collision2D other)
    {
        foreach (Collision2D c in collisions) {
            // even if the contact points are different they still match
            if (c.Equals(other)) {
                collisions.Remove(c);
                return;
            }
        }
    }
    
    public void OnCollisionStay2D(Collision2D other)
    {
        collisions.Add(other);
    }
    
    // only call after yield return new WaitForFixedUpdate() within a coroutine
    // pass in whatever function you want to do per contact point
    // (the function should usually be Entity's EvaluateCollision which you override with what you want)
    public void IterateOverCollisions(Action<Collision2D, ContactPoint2D> function)
    {
        if (collisions.Any()) {
            foreach (Collision2D coll in collisions) {
                for (int i = 0; i < coll.contactCount; i++) {
                    ContactPoint2D contact = coll.GetContact(i);
                    function(coll, contact);
                }
            }
        }
    }
}
