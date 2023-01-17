using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TDC.Core.Manager;
using UnityAsync;
using UnityEngine;

namespace TDC.UI.Shop
{
    public class BaaphieBlink : MonoBehaviour
    {
        public Animator Animator;
        private static readonly int _Blink = Animator.StringToHash("Blink");
        private CancellationTokenSource _TokenSource = new CancellationTokenSource();

        private void Start()
        {
            GameManager.RunOnInitialisation(Initialise);
        }

        private void Initialise()
        {
            // GameManager.SceneLoader.OnSceneLoadStarted += OnSceneLoadStart;
            BlinkLoop(_TokenSource.Token);
        }
        
        // private void OnSceneLoadStart(SceneEntry sceneEntry)
        // {
        //     GameManager.SceneLoader.OnSceneLoadStarted -= OnSceneLoadStart;
        //     _TokenSource.Cancel();
        //     _TokenSource = new CancellationTokenSource();
        // }

        private void OnDestroy()
        {
            _TokenSource.Cancel();
        }

        private async void BlinkLoop(CancellationToken token)
        {
            var rnd = new System.Random();
            try
            {
                while (!token.IsCancellationRequested)
                {
                    await Await.Seconds(rnd.Next(5,15)).ConfigureAwait(token);
                    Animator.SetTrigger(_Blink);
                }
            }
            catch (OperationCanceledException) {}
            catch (Exception e)
            {
                Debug.LogError(e);
            }
        }
    }
}
