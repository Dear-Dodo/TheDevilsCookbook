using System.Collections.Generic;
using System.Linq;
using TDC.Core.Type;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TDC.Core.Utility
{
    public static class ApplicationInitialiser
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialise()
        {
            SceneManager.sceneLoaded += RunApplicationStartEvents;
        }

        private static void RunApplicationStartEvents(Scene _, LoadSceneMode __)
        {
            IEnumerable<IApplicationStartHandler> handlers = Object.FindObjectsOfType<MonoBehaviour>(true)
                .OfType<IApplicationStartHandler>();
            foreach (IApplicationStartHandler handler in handlers)
            {
                handler?.ApplicationStart();
            }
        }
    }
}