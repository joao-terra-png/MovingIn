using System;
using UnityEngine;

namespace MovingIn.Hub{
    public enum Emote { None, Exclaim, Question, Dots, Sleep, Note }

    [Serializable]
    public class DialogueLine
    {
        public Speaker speaker;
        [TextArea] public string text;
        public Emote emote;
        public bool waitForInput;

        [NonSerialized] public bool lockPlayer;
        [NonSerialized] public Action onDone;

        public DialogueLine() { }

        public DialogueLine(Speaker speaker, string text, Emote emote = Emote.None, bool waitForInput = false)
        {
            this.speaker = speaker;
            this.text = text;
            this.emote = emote;
            this.waitForInput = waitForInput;
        }

        public DialogueLine Copy()
        {
            return new DialogueLine(speaker, text, emote, waitForInput);
        }
    }
}
