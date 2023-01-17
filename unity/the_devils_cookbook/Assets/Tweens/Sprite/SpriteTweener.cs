using DG.Tweening;
using UnityEngine;

namespace TDC.Tweens
{
    public abstract class SpriteTweener<T> : Tweener<T>
    {
        [HideInInspector] public SpriteRenderer SpriteRenderer;

        public override void Stop() => DOTween.Kill(SpriteRenderer);

        private void OnDestroy() => Stop();

        public void Awake() => SpriteRenderer = GetComponent<SpriteRenderer>();
    }
}