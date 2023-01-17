using DG.Tweening;
using UnityEngine;

namespace TDC.Tweens
{
    public abstract class ImageTweener<T> : Tweener<T>
    {
        [HideInInspector] public UnityEngine.UI.Image imageRenderer;

        public override void Stop() => DOTween.Kill(imageRenderer);

        private void OnDestroy() => Stop();

        public void Awake() => imageRenderer = GetComponent<UnityEngine.UI.Image>();
    }
}