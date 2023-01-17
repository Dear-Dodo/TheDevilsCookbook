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
    /// Tweens the rotation of the game object.
    /// </summary>
    public class DoRotation : TransformTweener<Vector3>
    {
		/// <summary>
		/// Tweens the rotation of the game object.
		/// </summary>
		public override void Play(Vector3 target) => transform.DORotate(target, Duration).SetEase(Ease);

        public override void Reset() => transform.eulerAngles = From;
    }
}