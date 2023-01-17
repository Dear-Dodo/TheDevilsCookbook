using System;
using UnityEngine;

namespace TDC.UI.Windowing
{
    [Serializable]
    public class WindowEntry
    {
        /// <summary>
        /// Instance of the window if IsSingle. Runtime variable only
        /// </summary>
        [NonSerialized] public Window Instance = null;

        public Window Window;

        [Tooltip("Whether or not this window is the only one of its kind to exist.")]
        public bool IsSingle;

        [Tooltip("If this window already exists in the scene, does it start active?")]
        public bool InitialState;

        [Tooltip("If this window already exists in the scene, what UI layer does it default to?")]
        public int InitialLayer;
    }
}