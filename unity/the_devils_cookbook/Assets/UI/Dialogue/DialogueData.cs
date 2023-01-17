using System;
using UnityEngine;

namespace TDC.UI.Dialogue
{
    [CreateAssetMenu(menuName = "TDC/Dialogue/New DialogueData")]
    public class DialogueData : ScriptableObject
    {
        [Serializable]
        public class Message
        {
            public string CharacterName;
            [TextArea]
            public string Text;
            public Sprite Portrait;
        }
        public Message[] Messages;
    }
}