using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class FireSprite : MainCharacter
{

    public Entity flameSpawn;
    public Transform flameSpawnFrom;
    public float flameSpeed;

    public float flameInterval = 0.08f;
    public float flameTimer = 0;

    [SerializeField] private float wallPushDistance = 2f;
    [SerializeField] private LayerMask wallPushMask;
    [SerializeField] private float wallPushStrength = 15f;
    [SerializeField] private float wallPushFalloff = 1f;

    private bool firstPush = false;

    protected new void Awake()
    {
        GameEvents.OnPrimaryAbilityDown += DoAbilityPrimaryDown;
        GameEvents.OnPrimaryAbilityHeld += DoAbilityPrimaryHold;
        GameEvents.OnMoveToUp += OnMoveToUp;
        base.Awake();
    }

    void OnDestroy()
    {
        GameEvents.OnPrimaryAbilityDown -= DoAbilityPrimaryDown;
        GameEvents.OnPrimaryAbilityHeld -= DoAbilityPrimaryHold;
        GameEvents.OnMoveToUp -= OnMoveToUp;
    }
    
    public override void DoAbilityPrimaryDown(object sender, Vector3Args args)
    {
        if (!CanDoAbility()) return;
        flameTimer = flameInterval;
        firstPush = true;
    }
    
    public override void DoAbilityPrimaryHold(object sender, Vector3Args args)
    {
        if (!CanDoAbility()) return;
        flameTimer += Time.deltaTime;
        if (flameTimer < flameInterval) return;
        
        Vector3 origin = flameSpawnFrom.position;
        Vector2 direction = args.pos - origin;
        direction = direction.normalized * flameSpeed;
        Entity newFlame = Instantiate(flameSpawn, origin, Quaternion.identity);
        // this is gross
        newFlame.SetVelocity(direction);

        flameTimer = 0;
        
        FirePush(origin, direction);
    }

    public override void DoAbilityPrimaryUp(object sender, Vector3Args args)
    {
        if (controlMode == controlModes.TopHalf) {
            GameEvents.InvokePlayerRegainFullControl();
        }
    }

    private void FirePush(Vector2 origin, Vector2 direction)
    {
        if (!firstPush) return;

        RaycastHit2D ray_hit = Physics2D.Raycast(origin, direction, wallPushDistance, wallPushMask);
        if (ray_hit) {
            // this prevents wall climbing
            float angle = Vector2.Angle(-direction.normalized, ray_hit.normal);
            angle = angle * Mathf.Deg2Rad;
            // steeper angles are less effective
            float angle_scaling = Mathf.Max(Mathf.Cos(angle), 0f);
            float distance_scaling = 1 / (wallPushFalloff * ray_hit.distance + 1);
            Vector2 pushVelocity = -direction.normalized * angle_scaling * wallPushFalloff * wallPushStrength;
            // no double jumps
            if (!onGround || jumpCooldownTimer <= jumpCooldown) pushVelocity.y = 0;
            if (pushVelocity.y > 0) {
                onGround = false;
                coyoteTimeTimer = coyoteTime + 1;
                jumpCooldownTimer = 0;
                print("firejump");
            }
            AddVelocity(pushVelocity);
            firstPush = false;
        }

    }

    public override void OnMoveToUp(object sender, Vector3Args args)
    {
        if (readyToMove && (controlMode == controlModes.BottomHalf || controlMode == controlModes.NPC)) {
            MoveToPoint(args.pos);
        }
    }
}
