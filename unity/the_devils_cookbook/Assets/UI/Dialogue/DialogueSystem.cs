using System;
using System.Threading.Tasks;
using Nito.AsyncEx;
using TDC.Core.Manager;
using TDC.UI.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TDC.UI.Dialogue
{
    [Serializable]
    public class DialogueSystem : GameManagerSubsystem
    {
        [SerializeField] private DialogueBox _DialogueBoxPrefab;
        private DialogueBox _DialogueBoxInstance;
        
        private IDialogueWriter _DialogueWriter;

        private DialogueData _DialogueData;

        private int _CurrentPromptIndex;

        protected override Task OnInitialise()
        {
            GameManager.SceneLoader.OnSceneLoadStarted += OnSceneLoadStart;
            return Task.CompletedTask;
        }

        private void OnSceneLoadStart(SceneEntry _)
        {
            if (_DialogueBoxInstance) Object.Destroy(_DialogueBoxInstance);
        }


        public void Register(IDialogueWriter dialogueWriter)
        {
            if (_DialogueWriter == null)
            {
                Debug.LogError($"DialogueSystem: Dialogue Writer has already been set.");
                return;
            }
            _DialogueWriter = dialogueWriter;
        }   

        public async Task Run(DialogueData dialogueData, int charactersPerSecond = 50)
        {
            if (!_DialogueBoxInstance)
            {
                _DialogueBoxInstance = Object.Instantiate(_DialogueBoxPrefab);
            }
            _DialogueBoxInstance.gameObject.SetActive(true);
            var writer = new TMPTypeWriter();

            var moveNext = new AsyncAutoResetEvent();
            
            void ContinueDialogue()
            {
                if (writer.IsRunning) writer.Skip();
                else moveNext.Set();
            }

            _DialogueBoxInstance.ContinuePressed += ContinueDialogue;
            _DialogueData = dialogueData;
            _CurrentPromptIndex = 0;

            while (_CurrentPromptIndex < _DialogueData.Messages.Length)
            {
                DialogueData.Message currentMessage = _DialogueData.Messages[_CurrentPromptIndex];
                _DialogueBoxInstance.SetPortrait(currentMessage.Portrait);
                _DialogueBoxInstance.SetName(currentMessage.CharacterName);
                writer.Initialise(currentMessage.Text, 75, _DialogueBoxInstance.DialogueText);
                await writer.Start();
                await moveNext.WaitAsync();
                _CurrentPromptIndex++;
            }
            
            _DialogueBoxInstance.gameObject.SetActive(false);
            _DialogueBoxInstance.ContinuePressed -= ContinueDialogue;
        }
    }
}