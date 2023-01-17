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
    /// Tweens the position of a game object.
    /// </summary>
    public class DoTranslation : TransformTweener<Vector3>
    {
        /// <summary>
        /// Tweens the position of the game object.
        /// </summary>
        public override void Play(Vector3 target) => transform.DOMove(target, Duration).SetEase(Ease);

        public override void Reset() => transform.position = From;
    }
}