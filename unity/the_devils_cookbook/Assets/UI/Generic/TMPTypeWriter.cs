using System;
using System.Threading;
using System.Threading.Tasks;
using TDC.UI.Dialogue;
using TMPro;
using UnityAsync;

namespace TDC.UI.Generic
{
    public class TMPTypeWriter : IDialogueWriter
    {
        public string Text { get; private set; }
        public int CharactersPerSecond { get; private set; }
        public TextMeshProUGUI TextTarget { get; private set; }
        
        public bool IsRunning { get; private set; }

        private CancellationTokenSource _TokenSource = new CancellationTokenSource();
        
        public async Task Start()
        {
            if (!TextTarget) throw new Exception("Type writer not initialised before start.");
            IsRunning = true;
            var currentVisibleCharacters = 0;
            float interval = 1.0f / CharactersPerSecond;
            CancellationToken token = _TokenSource.Token;
            while (currentVisibleCharacters <= Text.Length)
            {
                TextTarget.maxVisibleCharacters = currentVisibleCharacters;
                currentVisibleCharacters++;
                try
                {
                    await Await.Seconds(interval).ConfigureAwait(token);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                
            }

            IsRunning = false;
        }

        public void Skip()
        {
            TextTarget.maxVisibleCharacters = Text.Length;
            Cancel();
        }

        public void Cancel()
        {
            _TokenSource.Cancel();
            IsRunning = false;
        }

        public void Initialise(string text, int charactersPerSecond, TextMeshProUGUI textTarget)
        {
            Text = text;
            CharactersPerSecond = charactersPerSecond;
            TextTarget = textTarget;
            _TokenSource = new CancellationTokenSource();
            textTarget.text = text;
            textTarget.maxVisibleCharacters = 0;
        }

        public TMPTypeWriter(string text, int charactersPerSecond, TextMeshProUGUI textTarget)
        {
            Text = text;
            CharactersPerSecond = charactersPerSecond;
            TextTarget = textTarget;
            textTarget.text = text;
            textTarget.maxVisibleCharacters = 0;
        }
        public TMPTypeWriter() {}
    }
}