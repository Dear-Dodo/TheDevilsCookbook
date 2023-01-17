using UnityEngine;

namespace TDC.Core.Extension
{
    public static class GameObjectEx
    {
        public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component
        {
            // try get the component, otherwise add the component.
            if (!gameObject.TryGetComponent<T>(out var component))
            {
                component = gameObject.AddComponent<T>();
            }
            return component;
        }

        public static bool TryGetComponentInParent<T>(this Component input, out T component) where T : Component
        {
            component = input.GetComponentInParent<T>();
            return component != null;
        }
    }
}