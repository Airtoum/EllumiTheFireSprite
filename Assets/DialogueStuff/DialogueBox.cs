using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueBox : MonoBehaviour
{
    public Queue<page> pages = new Queue<page>();
    
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI DialogueText;

    //public Animator animator;

    private bool triggered = false;

    private bool still_typing = false;

    void Awake()
    {
        GameEvents.StartDialogue += StartDialogue;
    }

    public void StartDialogue(object sender, DialogueArgs args)
    {
        Dialogue dialogue = args.dialogue;
        MainCharacter mainCharacter = args.main_char;

        triggered = true;
        //animator.SetBool("isOpen", true);
        //nameText.text = dialogue.name;

        pages.Clear();

        foreach (page current_page in dialogue.pages)
        {
            pages.Enqueue(current_page);
        }

        
        //DisplayNextSentence();
        StartCoroutine(TriggerDialoguesSequentially());
    }

    IEnumerator TriggerDialoguesSequentially()
    {
        DisplayNextSentence();
        // yield return null waits until next frame; unnecessary? 
        yield return null;
        while (triggered)
        {
            yield return new WaitUntil(() => Input.anyKeyDown);
            DisplayNextSentence();
            yield return null;
        }
        //GameManager.Instance.state = GameState.FREEWALK;
    }
    
    public void DisplayNextSentence()
    {
        if(pages.Count == 0)
        {
            EndDialogue();
            return;
        }

        if (still_typing) {
            StopCoroutine("TypeSentence");
            return;
        }
        
        page page = pages.Dequeue();

        nameText.text = page.speaker;
        nameText.color = page.speakerColor;

        //Debug.Log(sentence);
        StartCoroutine(TypeSentence(page));
    }

    IEnumerator TypeSentence(page page) 
    {
        DialogueText.text = "";
        still_typing = true;
        foreach (char letter in page.text.ToCharArray())
        {
            if (letter != '|') {
                DialogueText.text += letter;
            }
            yield return new WaitForSeconds(page.typeInterval);
        }
        still_typing = false;
    }
    
    void EndDialogue()
    {
        triggered = false;
        Debug.Log("End of conversation");
        //animator.SetBool("isOpen", false);
    }
}
