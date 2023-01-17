using UnityEngine;
using UnityEngine.Events;

namespace TDC.Core.EventHelpers

{
    /// <summary>
    /// Base Component to trigger events.
    /// </summary>
    public class EventHelper : MonoBehaviour
    {
        /// <summary>
        /// Base EventHandle for all event helpers.
        /// </summary>
        [field: SerializeField] public UnityEvent EventHandle { get; set; }

        /// <summary>
        /// Invokes the EventHandle.
        /// </summary>
        public virtual void Execute() => EventHandle?.Invoke();
    }
}