using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
[CreateAssetMenu(fileName = "New Dialogue", menuName = "Dialogue")]
public class Dialogue : ScriptableObject
{
    [TextArea(3, 10)]
    public string[] sentences;
    [SerializeField]
    public page[] pages;
}

[System.Serializable]
public struct page
{
    [Tooltip("Use vertical bars \"|\" for pauses")]
    public string text;
    public string speaker;
    [ColorUsage(false)]
    public Color speakerColor;
    //public bool hasOptions;
    public option[] options;
    [Tooltip("Number of seconds between characters being typed.")]
    public float typeInterval;
}

[System.Serializable]
public struct option
{
    public string text;
    public string action;
}
