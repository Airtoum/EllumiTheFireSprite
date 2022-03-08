using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public bool entered = false;
    public Dialogue dialogue;
    public MainCharacter character;

    public bool oneOff = true;
    public bool triggeredYet = false;

    void Update() {
        if (entered /*&& Input.GetKeyUp(KeyCode.E)*/ /*&& !FindObjectOfType<DialogueManager>().triggered*/) {
            triggeredYet = true;
            TriggerDialogue();
            //StartCoroutine(TriggerNextDialogue());
            enabled = false;
        }
    }

    public void TriggerDialogue()
    {
        GameEvents.InvokeStartDialogue(character, dialogue);
        //GameManager.Instance.state = GameState.DIALOG;
    }

    /*
    IEnumerator TriggerNextDialogue()
    {
        yield return null;
        while (FindObjectOfType<DialogueManager>().triggered)
        {
            yield return new WaitUntil(() => Input.GetKeyUp(KeyCode.Space));
            FindObjectOfType<DialogueManager>().DisplayNextSentence();
            yield return null;
        }
        //GameManager.Instance.state = GameState.FREEWALK;
    }
    */

    void OnTriggerStay2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            entered = true;
        }
    }

    void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            entered = false;
        }
    }
}
