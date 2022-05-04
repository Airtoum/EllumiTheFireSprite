using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerTalkCheck : MonoBehaviour
{

    public int highestTalkPriority = 0;
    public Talkable wouldTalkTo = null;
    
    // Start is called before the first frame update
    void Start()
    {
        GameEvents.BroadcastTalkable += OnBroadcastTalkable;
    }
    
    void OnDestroy()
    {
        GameEvents.BroadcastTalkable -= OnBroadcastTalkable;
    }

    // Update is called once per frame
    void Update()
    {
        highestTalkPriority = 0;
        wouldTalkTo = null;
        GameEvents.InvokeScanForTalkable(transform.position);
        GameEvents.InvokeATalkableHasBeenChosen(wouldTalkTo);
    }

    private void OnBroadcastTalkable(object sender, TalkableArgs args)
    {
        IsTopPriority(args.person, args.person.priority);
    }

    public bool IsTopPriority(Talkable ofWhom, int priority)
    {
        if (priority > highestTalkPriority || wouldTalkTo == ofWhom) {
            highestTalkPriority = priority;
            wouldTalkTo = ofWhom;
            return true;
        }
        return false;
    }
}
