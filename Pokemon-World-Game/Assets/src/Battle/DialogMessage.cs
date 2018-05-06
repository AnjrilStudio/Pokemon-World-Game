using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


public class DialogMessage
{
    public string Text { get; }
    public bool Confirm { get; } //indique si besoin de confirmer (skip) pour voir la suite des dialogues

    public DialogMessage(string text, bool confirm)
    {
        Text = text;
        Confirm = confirm;
    }
}
