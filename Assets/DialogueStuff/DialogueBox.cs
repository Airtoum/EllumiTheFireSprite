using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.PlayerLoop;

public class DialogueBox : MonoBehaviour
{
    public Dialogue current_dialogue;
    public Queue<page> pages = new Queue<page>();
    
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI DialogueText;
    public UnityEngine.UI.Image dialogueBox;
    public GameObject optionsBox;
    public TextMeshProUGUI optionText1;
    public TextMeshProUGUI optionText2;
    public TextMeshProUGUI optionText3;
    public UnityEngine.UI.Image selector1;
    public UnityEngine.UI.Image selector2;
    public UnityEngine.UI.Image selector3;

    //public Animator animator;

    private bool triggered = false;

    private bool still_typing = false;
    private Coroutine still_typing_what;
    private page last_page;

    private int which_selector = 1;
    private bool are_there_options = false;

    private MainCharacter helper;

    void Awake()
    {
        GameEvents.StartDialogue += StartDialogue;
    }

    void Start()
    {
        Hide();
    }

    public void StartDialogue(object sender, DialogueArgs args)
    {
        Show();
        current_dialogue = args.dialogue;
        helper = args.main_char;

        triggered = true;
        //animator.SetBool("isOpen", true);
        //nameText.text = dialogue.name;

        pages.Clear();

        foreach (page current_page in current_dialogue.pages)
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
            if (are_there_options) {
                if (Input.GetAxis("Down") > 0) {
                    which_selector = Ultramath.MathMod(which_selector + 1 - 1, 3) + 1;
                    UpdateSelector();
                } else if (Input.GetAxis("Up") > 0) {
                    which_selector = Ultramath.MathMod(which_selector - 1 - 1, 3) + 1;
                    UpdateSelector();
                }
            }
            if (InputAdvanceDialogue()) {
                DisplayNextSentence();
            }
            yield return null;
        }
        //GameManager.Instance.state = GameState.FREEWALK;
    }
    
    public void DisplayNextSentence()
    {
        if (are_there_options) {
            if (MakeSelection()) return;
        }
        
        if(pages.Count == 0)
        {
            EndDialogue();
            return;
        }

        if (still_typing) {
            StopCoroutine(still_typing_what);
            DialogueText.text = last_page.text.Replace("|", "");
            still_typing = false;
            return;
        }
        
        page page = pages.Dequeue();

        nameText.text = page.speaker;
        nameText.color = page.speakerColor;

        if (page.options.Length > 0) {
            ShowOptions();
            optionText1.text = page.options[0].text;
            optionText2.text = page.options[1].text;
            optionText3.text = page.options[2].text;
            which_selector = 1;
            UpdateSelector();
            are_there_options = true;
        } else {
            are_there_options = false;
            HideOptions();
        }

        //Debug.Log(sentence);
        last_page = page;
        still_typing_what = StartCoroutine(TypeSentence(page));
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
        still_typing = false;
        pages.Clear();
        are_there_options = false;
        StopAllCoroutines();
        GameEvents.InvokeEndDialogue();
        Hide();
        //animator.SetBool("isOpen", false);
    }

    private void Show()
    {
        nameText.enabled = true;
        DialogueText.enabled = true;
        dialogueBox.enabled = true;
        print("show");
    }
    
    private void Hide()
    {
        nameText.enabled = false;
        DialogueText.enabled = false;
        dialogueBox.enabled = false;
        HideOptions();
        print("hide");
    }

    private void ShowOptions()
    {
        optionsBox.SetActive(true);
        optionText1.enabled = true;
        optionText2.enabled = true;
        optionText3.enabled = true;
    }
    
    private void HideOptions()
    {
        optionsBox.SetActive(false);
        optionText1.enabled = false;
        optionText2.enabled = false;
        optionText3.enabled = false;
        selector1.enabled = false;
        selector2.enabled = false;
        selector3.enabled = false;
    }

    private void UpdateSelector()
    {
        selector1.enabled = false;
        selector2.enabled = false;
        selector3.enabled = false;
        if (which_selector == 1) {
            selector1.enabled = true;
        }
        else if (which_selector == 2) {
            selector2.enabled = true;
        }
        else if (which_selector == 3) {
            selector3.enabled = true;
        }
    }

    bool InputAdvanceDialogue()
    {
        return (Input.GetAxis("Jump") > 0 || Input.GetAxis("Talk") > 0);
    }

    private bool MakeSelection()
    {
        string command = "";
        command = last_page.options[which_selector - 1].action;

        switch (command) {
            case "None":
                break;
            case "Move":
                GameEvents.InvokeSelectPositionPlayerControls(helper.gameObject);
                EndDialogue();
                return true;
                break;
            case "Ability":
                GameEvents.InvokeUnpairPlayerControls(helper.gameObject);
                EndDialogue();
                return true;
                break;
            case "Quit":
                EndDialogue();
                return true;
                break;
            default:
                break;
        }
        return false;
    }
    
}
