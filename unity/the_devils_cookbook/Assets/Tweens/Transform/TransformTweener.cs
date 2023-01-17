using DG.Tweening;

namespace TDC.Tweens
{
    public abstract class TransformTweener<T> : Tweener<T>
    {

        private void OnDestroy() => Stop();
        /// <summary>
        /// This will stop any tweens on this transform.
        /// </summary>
        public override void Stop() => DOTween.Kill(transform);
    }
}