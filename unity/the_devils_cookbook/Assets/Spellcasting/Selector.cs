using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TDC.Core.Manager;
using UnityAsync;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.VFX;

namespace TDC.Spellcasting
{
    public abstract class Selector : MonoBehaviour
    {
        protected const int VFXDestroyWaitTime = 5;
        protected const int CastableSurfaceBitMask = 1 << 6;
        public static readonly int ColourID = Shader.PropertyToID("_Color");

        // Selector components
        protected Camera MainCamera;

        public VisualEffect VisualEffect { get; protected set; }
        public Renderer Renderer;

        // Runtime
        protected readonly AsyncAutoResetEvent ConfirmPressedAsync = new AsyncAutoResetEvent();

        protected bool IsCurrentlySelecting = false;
        protected Target LastSelectedTarget;
        public List<Target> Targets { get; protected set; }
        protected Transform Source;
        protected CancellationToken Token;

        public readonly AsyncManualResetEvent SelectionFinishedAsync = new AsyncManualResetEvent();

        public class SelectorSettings
        {
            public bool ShouldDestroyImmediately = false;
            public int TargetCount = 1;
        }

        protected abstract SelectorSettings _Settings { get; set; }
        public SelectorSettings Settings { get => _Settings; set => _Settings = value; }

        /// <summary>
        /// Begin a selection operation. Returns once complete or EndSelect() is called.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<Target[]> StartSelection(Transform source, CancellationToken token)
        {
            Assert.IsFalse(IsCurrentlySelecting);
            SelectionFinishedAsync.Reset();
            IsCurrentlySelecting = true;
            Token = token;
            Targets = new List<Target>(Settings.TargetCount);
            Source = source;
            GameManager.PlayerControls.Player.Fire.performed += ConfirmSelect;
            OnSelectionStart();
            try
            {
                await SelectTargets(token);
            }
            catch (OperationCanceledException) { }
            EndSelection();
            token.ThrowIfCancellationRequested();
            return Targets.ToArray();
        }

        /// <summary>
        /// End the current selection operation.
        /// </summary>
        public void EndSelection()
        {
            SelectionFinishedAsync.Set();
            IsCurrentlySelecting = false;
            GameManager.PlayerControls.Player.Fire.performed -= ConfirmSelect;
            OnSelectionEnd();
            if (Settings.ShouldDestroyImmediately)
            {
                Destroy(gameObject);
                return;
            }
            DestroyOnVFXFinish(1);
        }

        public virtual void SetGraphics(Spell.SpellGraphics graphics)
        {
            SetRendererGraphics(graphics);
            SetVFXGraphics(graphics);
        }

        public virtual async void DestroyOnVFXFinish(int frameDelay = 0)
        {
            if (VisualEffect == null)
            {
                Destroy(gameObject);
                return;
            }
            if (Renderer != null) Renderer.enabled = false;
            IsCurrentlySelecting = false;
            VisualEffect.Stop();

            if (frameDelay > 0) await Await.Updates(frameDelay);

            if (VisualEffect.visualEffectAsset == null)
            {
                Destroy(gameObject);
                return;
            }

            if (SystemInfo.graphicsDeviceType == GraphicsDeviceType.OpenGLCore)
            {
                await Task.Delay(VFXDestroyWaitTime);
            }

            while (true)
            {
                if (VisualEffect.aliveParticleCount == 0)
                {
                    Destroy(gameObject);
                    return;
                }

                await Task.Delay(1000);
            }
        }

        protected virtual void SetRendererGraphics(Spell.SpellGraphics graphics)
        {
            if (graphics.Decal != null)
            {
                Renderer.material.mainTexture = graphics.Decal;
                Renderer.material.SetColor(ColourID, graphics.DecalColour);
                Renderer.enabled = true;
            }
            else Renderer.enabled = false;
        }

        protected virtual void SetVFXGraphics(Spell.SpellGraphics graphics)
        {
            if (graphics.VFX != null)
            {
                VisualEffect.visualEffectAsset = graphics.VFX;
                VisualEffect.enabled = true;
            }
            else VisualEffect.enabled = false;
        }

        protected virtual void OnSelectionStart()
        { }

        protected virtual void OnSelectionEnd()
        { }

        protected async Task SelectTargets(CancellationToken token)
        {
            while (Targets.Count < Settings.TargetCount && IsCurrentlySelecting)
            {
                await ConfirmPressedAsync.WaitAsync(token);
                token.ThrowIfCancellationRequested();
                if (LastSelectedTarget != null) RequestTargetAdd(LastSelectedTarget);
            }
        }

        private void ConfirmSelect(InputAction.CallbackContext _)
        {
            ConfirmPressedAsync.Set();
        }

        /// <summary>
        /// Handle request to add target from user click. Validation should be here.
        /// </summary>
        /// <param name="toAdd"></param>
        protected abstract void RequestTargetAdd(Target toAdd);

        /// <summary>
        /// Update _LastTarget and the graphical representation of the selector.
        /// This is run each Update().
        /// </summary>
        protected abstract void UpdateSelection();

        protected virtual void Awake()
        {
            MainCamera = Camera.main;
            VisualEffect = GetComponent<VisualEffect>();
            Renderer = GetComponent<Renderer>();
        }

        protected virtual void Update()
        {
            if (!IsCurrentlySelecting) return;
            UpdateSelection();
        }
    }
}