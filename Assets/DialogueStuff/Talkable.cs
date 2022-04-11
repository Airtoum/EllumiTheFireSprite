using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Talkable : MonoBehaviour
{
    [SerializeField] public GameObject TalkNotifierGameObject;
        
    public List<Dialogue> Conversations;
    public int conversationIndex = 0;
    public float radius;
    public int priority;
    private TalkNotifier myTalkNotifier = null;
    public MainCharacter optionalCharacter = null;
    private bool isAlreadyTalking = false;
    
    // Start is called before the first frame update
    void Start()
    {
        GameEvents.StartDialogue += OnStartDialogue;
        GameEvents.EndDialogue += OnEndDialogue;
        GameEvents.ScanForTalkable += OnScanForTalkable;
        GameEvents.ATalkableHasBeenChosen += OnATalkableHasBeenChosen;
        conversationIndex = Mathf.Clamp(conversationIndex, 0, Conversations.Count - 1);
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnATalkableHasBeenChosen(object sender, TalkableArgs args)
    {
        bool talk_to = false;
        if (!isAlreadyTalking) {
            /*
            // detect if player is in range
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, radius);
            foreach (Collider2D coll in colliders) {
                if (!coll.CompareTag("Player")) continue;
                PlayerTalkCheck playerTalkCheck = coll.gameObject.GetComponent<PlayerTalkCheck>();
                if (playerTalkCheck) {
                    if (playerTalkCheck.IsTopPriority(this, priority)) {
                        talk_to = true;
                    }
                }
            }*/
            talk_to = args.person == this;
        }

        if (talk_to) {
            // manage talk notifier
            if (myTalkNotifier) {
                ;
            } else {
                myTalkNotifier = Instantiate(TalkNotifierGameObject, transform.position, Quaternion.identity)
                    .GetComponent<TalkNotifier>();
            }

            // start dialogue when player presses e
            if (Input.GetKeyDown(KeyCode.E)) {
                TriggerDialogue();
            }
        } else {
            if (myTalkNotifier) {
                // make this better
                myTalkNotifier.GoAway();
            }
        }
    }

    private bool IsInRange(Vector3 point)
    {
        Vector2 point_flat = point;
        Vector2 pos_flat = transform.position;
        return Vector2.Distance(point_flat, pos_flat) <= radius;
    }
    
    public void TriggerDialogue()
    {
        if (Conversations.Count > 0) {
            GameEvents.InvokeStartDialogue(optionalCharacter, Conversations[conversationIndex]);
            conversationIndex = Mathf.Clamp(conversationIndex + 1, 0, Conversations.Count - 1);
        }
    }

    private void OnStartDialogue(object sender, DialogueArgs args)
    {
        isAlreadyTalking = true;
    }

    private void OnEndDialogue(object sender, EventArgs args)
    {
        isAlreadyTalking = false;
    }

    private void OnScanForTalkable(object sender, Vector3Args args)
    {
        if (IsInRange(args.pos)) GameEvents.InvokeBroadcastTalkable(transform.position, this);
    }
}
