/*
 *  Author: James Greensill
 *  Usage:  Tweening.
 */

// External Namespaces
using DG.Tweening;
using UnityEngine;

namespace TDC.Tweens
{
    /// <summary>
    /// Tweens the local position of a game object.
    /// </summary>
    public class DoLocalTranslation : TransformTweener<Vector3>
    {
        /// <summary>
        /// Tweens the local position of the game object.
        /// </summary>

        public override void Play(Vector3 target) => transform.DOLocalMove(target, Duration).SetEase(Ease);

        public override void Reset() => transform.localPosition = From;
    }
}