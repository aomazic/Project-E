using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dialogue
{
    public string Speaker { get; set; }
    public string Text { get; set; }

    public Dialogue(string speaker, string text)
    {
        Speaker = speaker;
        Text = text;
    }
}
