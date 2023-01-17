/*
 *  Author: James Greensill & Lewis Comstive
 *  Usage:  Tweening interface
 */


using UnityEngine;

namespace TDC.Tweens
{
	/// <summary>
	/// Tweening Interface for all tweening classes.
	/// </summary>
	public interface ITweener
	{
		/// <summary>
		/// Duration of the Tween.
		/// </summary>
        public float Duration { get; set; }

		/// <summary>
		/// Easing Curve of the Tween.
		/// </summary>
		public AnimationCurve Ease { get; set; }

		/// <summary>
		/// Play the Tween.
		/// </summary>
		public void Play();

		/// <summary>
		/// Rewind the Tween.
		/// </summary>
		public void Rewind();

		/// <summary>
		/// Stops the tween
		/// </summary>
		public void Stop();

		/// <summary>
		/// Sets the tweened property to it's original pre-tween state
		/// </summary>
		public void Reset();
	}
}