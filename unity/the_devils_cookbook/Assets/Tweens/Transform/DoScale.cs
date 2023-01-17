/*
 *  Author: James Greensill
 *  Usage:  Tweening.
 */

using DG.Tweening;
using UnityEngine;

namespace TDC.Tweens
{
    /// <summary>
    /// Tweens the scale of the game object.
    /// </summary>
    public class DoScale : TransformTweener<Vector3>
    {
		/// <summary>
		/// Tweens the scale of the game object.
		/// </summary>
		public override void Play(Vector3 target) => transform.DOScale(target, Duration).SetEase(Ease);

        public override void Reset() => transform.localScale = From;
    }
}