using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class DebugText : MonoBehaviour
{
    // this is just a global variable. use for ui debugging.
    private static string text;

    public static void SetText(string debug_text)
    {
        text = debug_text;
    }
    
    public static string GetText()
    {
        return text;
    }

    private TextMeshProUGUI textmeshpro;
    
    private void Start()
    {
        textmeshpro = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        textmeshpro.text = text;
    }
}
