/*
 *  Author:     James Greensill
 *  Purpose:    Singleton class for single objects
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.AddressableAssets.ResourceLocators;
using UnityEngine.ResourceManagement.ResourceLocations;

namespace TDC.Core.Type
{
    /// <summary>
    /// Denotes that a singleton is persistent and should not be destroyed on load.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class PersistentAttribute : Attribute
    { }

    /// <summary>
    /// Denotes that a singleton should be initialised / created at runtime initialisation.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class SingletonInitializeOnRuntime : Attribute
    { }

    /// <summary>
    /// Provides an Addressables key for the prefab used to instantiate the singleton.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AddressablePrefabAttribute : Attribute
    {
        public readonly string Address;

        public AddressablePrefabAttribute(string address)
        {
            Address = address;
        }
    }

    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance
        {
            get
            {
                if (_Instance != null) return _Instance;
                throw new NullReferenceException($"No instance of singleton '{typeof(T).Name}' exists.");
            }
        }

        /// <summary>
        /// This is executed immediately after the singleton is initialized.
        /// </summary>
        public event Action OnInitialized;

        private static T _Instance;

        protected static async Task InitialiseAuto(IResourceLocator locator)
        {
            if (!typeof(T).IsDefined(typeof(SingletonInitializeOnRuntime))) return;
            await Initialise(locator);
        }

        /// <summary>
        /// Initialises the singleton for this type.
        /// </summary>
        public static async Task Initialise(IResourceLocator locator)
        {
            bool isPersistent = typeof(T).IsDefined(typeof(PersistentAttribute));

            var prefabAttribute = typeof(T).GetCustomAttribute<AddressablePrefabAttribute>();

            if (prefabAttribute != null)
            {
                try
                {
                    if (!locator.Locate(prefabAttribute.Address, typeof(GameObject),
                            out IList<IResourceLocation> locations))
                    {
                        Debug.LogError($"Unable to locate prefab '{prefabAttribute.Address}' for singleton '{typeof(T).Name}'.");
                    }

                    foreach (IResourceLocation resourceLocation in locations)
                    {
                        Debug.Log(resourceLocation);
                    }

                    Task<GameObject> prefabLoadTask = Addressables.LoadAssetAsync<GameObject>(locations.First()).Task;
                    await prefabLoadTask;
                    GameObject prefab = prefabLoadTask.Result;
                    _Instance = Instantiate(prefab).GetComponent<T>();
                }
                catch (Exception)
                {
                    Debug.LogError($"Prefab key '{prefabAttribute.Address}' for Singleton '{typeof(T).Name}' not found in Addressables.");
                    throw;
                }
            }
            else
            {
                _Instance = new GameObject().AddComponent<T>();
            }

            if (isPersistent) DontDestroyOnLoad(_Instance.gameObject);
            Debug.Log($"Initialised Singleton for '{typeof(T)}'.");

            if (_Instance is Singleton<T> instance)
            {
                instance.OnInitialize();
                instance.OnInitialized?.Invoke();
            }
        }

        /// <summary>
        /// If Instance is null then set Instance to this, otherwise destroy this.
        /// </summary>
        protected virtual void Awake()
        {
            if (_Instance != this && _Instance != null)
            {
                Debug.LogWarning($"Second instance of singleton {typeof(T)} was created.");
                Destroy(gameObject);
            }
            OnAwake();
        }

        public virtual void OnInitialize() {}

        /// <summary>
        /// Allows child classes to access awake functionallity
        /// </summary>
        protected virtual void OnAwake() {}
    }
}