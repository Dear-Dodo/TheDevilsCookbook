/*
 *  Author: James Greensill & Lewis Comstive
 *  Usage:  Tweening.
 */

// External Namespaces
using UnityEngine;

namespace TDC.Tweens
{
	/// <summary>
	/// Generic tween class.
	/// </summary>
	/// <typeparam name="T">Data type for tweening.</typeparam>
	public abstract class Tweener<T> : MonoBehaviour, ITweener
	{
		/// <summary>
		/// Duration of the tween.
		/// </summary>
		[field: SerializeField] public float Duration { get; set; }

		/// <summary>
		/// Ease Curve of the tween.
		/// </summary>
		[field: SerializeField] public AnimationCurve Ease { get; set; } = AnimationCurve.Linear(0, 0, 1, 1);

		/// <summary>
		/// Initial value of the tween.
		/// </summary>
		[field: SerializeField] public T From { get; set; }

		/// <summary>
		/// Target value of the tween.
		/// </summary>
		[field: SerializeField] public T To { get; set; }

		/// <summary>
		/// When true, resets the tween when script is enabled
		/// </summary>
		[field: SerializeField] public bool ResetOnEnable { get; set; }

		/// <summary>
		/// Abstract function to start tweening.
		/// </summary>
		public void Play() => Play(To);

		/// <summary>
		/// Plays the tween towards it's initial value
		/// </summary>
		public void Rewind() => Play(From);

		public abstract void Play(T target);

        public abstract void Stop();

		/// <summary>
		/// Sets the tweened property to <see cref="From"/>
		/// </summary>
		public virtual void Reset()
		{
			Stop();

			// Decrease duration to 0 for instant result
			float duration = Duration;
			Duration = 0;
			Rewind();

			// Reset duration to original value
			Duration = duration;
		}

		/// <summary>
		/// Resets the tween if <see cref="ResetOnEnable"/>
		/// </summary>
		protected virtual void OnEnable()
		{
			if (ResetOnEnable)
				Reset();
		}
	}
}