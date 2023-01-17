using System.Collections;
using UnityEngine;

namespace TDC.Core.EventHelpers
{
    public class PlayAfterTime : EventHelper
    {
        /// <summary>
        /// Time to wait after the game object is enabled before playing the event.
        /// </summary>
        [field: SerializeField] private float Time { get; set; } = 1.0f;

        private Coroutine m_Routine;

        /// <summary>
        /// Start the coroutine to play the event.
        /// </summary>
        private void OnEnable() => m_Routine = StartCoroutine(StartAfterTime());

        /// <summary>
        /// Stops the coroutine
        /// </summary>
        private void OnDisable() => StopCoroutine(m_Routine);

        /// <summary>
        /// Wait for the time and then play the event.
        /// </summary>
        /// <returns>IEnumerator time.</returns>
        private IEnumerator StartAfterTime()
        {
            yield return new WaitForSeconds(Time);
            Execute();
        }
    }
}