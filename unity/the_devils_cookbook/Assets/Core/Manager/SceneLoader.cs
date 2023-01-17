using FMODUnity;
using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using TDC.ThirdParty.SerializableDictionary;
using UnityAsync;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;

namespace TDC.Core.Manager
{
    public enum SceneType
    {
        Level,
        Utility
    }

    [Serializable]
    public class SceneEntry
    {
        public string SceneName;
    }

    [Serializable]
    public class SceneLoader : GameManagerSubsystem
    {
        [SerializeField] private SerializableDictionary<string, SceneEntry> _Scenes;

        public SerializableDictionary<string, SceneEntry> Scenes { get { return new SerializableDictionary<string, SceneEntry>(_Scenes); }}

        /// <summary>
        /// Invoked on start of scene load. Passes the scene name and whether it has level data.
        /// </summary>
        public event Action<SceneEntry> OnSceneLoadStarted;

        /// <summary>
        /// Invoked on successful scene load. Passes the scene name and whether it has level data.
        /// </summary>
        public event Action<SceneEntry> OnSceneLoadFinished;

        private SceneEntry GetScene(string name)
        {
            if (!_Scenes.TryGetValue(name, out SceneEntry sceneEntry))
                throw new KeyNotFoundException($"No scene with name '{name}' was registered.");
            return sceneEntry;
        }

        public bool TryGetScene(string name, out SceneEntry entry) => _Scenes.TryGetValue(name, out entry);

        /// <summary>
        /// Begins loading a scene and returns the AsyncOperation ready for scene activation.
        /// See: AsyncOperation.allowSceneActivation
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isAdditive"></param>
        /// <returns></returns>
        public async Task<AsyncOperation> PreLoadScene(string name, bool isAdditive = false)
        {
            SceneEntry entry = GetScene(name);

            AsyncOperation sceneLoad = UnityEngine.SceneManagement.SceneManager.LoadSceneAsync(entry.SceneName,
                isAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single);
            sceneLoad.allowSceneActivation = false;

            while (sceneLoad.progress < 0.9f)
            {
                await Await.NextUpdate();
            }

            return sceneLoad;
        }

        /// <summary>
        /// Load and activate a scene using the default load delay range.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="isAdditive"></param>
        public async Task LoadScene(string name, bool isAdditive = false)
        {
            try
            {
                SceneEntry entry = GetScene(name);
                if (entry != null)
                {
                    await LoadScene(entry, 0, null, isAdditive);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.Message);
            }
        }

        public async Task LoadScene(SceneEntry entry, float minLoadTimeMs, SceneEntry loadingScene = null,
            bool isAdditive = false)
        {
            var sceneLoadEvent = new AsyncAutoResetEvent();
            void OnSceneLoad(Scene _, LoadSceneMode __) => sceneLoadEvent.Set();

            AsyncOperation loadingSceneOperation = null;
            if (!isAdditive && loadingScene != null)
            {
                loadingSceneOperation = SceneManager.LoadSceneAsync(loadingScene.SceneName);
                loadingSceneOperation.allowSceneActivation = false;
            }

            RuntimeManager.GetBus("bus:/Master/SFX").stopAllEvents(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

            var loadTimer = Stopwatch.StartNew();

            OnSceneLoadStarted?.Invoke(entry);

            AsyncOperation sceneLoad = SceneManager.LoadSceneAsync(entry.SceneName,
                isAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single);

            sceneLoad.allowSceneActivation = false;
            if (loadingSceneOperation != null)
            {
                loadingSceneOperation.allowSceneActivation = true;
                await Await.Until(() => loadingSceneOperation.progress >= 1.0f);
            }

            SceneManager.sceneLoaded += OnSceneLoad;

            while (sceneLoad.progress < 0.9f || minLoadTimeMs > loadTimer.ElapsedMilliseconds)
            {
                await Await.NextUpdate();
            }

            loadTimer.Stop();
            sceneLoad.allowSceneActivation = true;

            await sceneLoadEvent.WaitAsync();
            SceneManager.sceneLoaded -= OnSceneLoad;
            OnSceneLoadFinished?.Invoke(entry);
        }

        /// <summary>
        /// Load and activate a scene, optionally with a minimum time before completing.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="loadingScene">Loading scene before loading target scene</param>
        /// <param name="isAdditive"></param>
        /// <param name="minLoadTimeMS"></param>
        // public async Task LoadScene(string name, float minLoadTimeMS, string loadingScene = "", bool isAdditive = false)
        // {
        //     var sceneLoadEvent = new AsyncAutoResetEvent();
        //     void OnSceneLoad(Scene _, LoadSceneMode __) => sceneLoadEvent.Set();
        //
        //     SceneEntry entry;
        //     AsyncOperation loadingScreen = null;
        //     if (!isAdditive && !string.IsNullOrEmpty(loadingScene))
        //     {
        //         entry = GetScene(loadingScene);
        //         loadingScreen = SceneManager.LoadSceneAsync(entry.Index);
        //         loadingScreen.allowSceneActivation = false;
        //     }
        //     var loadTimer = Stopwatch.StartNew();
        //     entry = GetScene(name);
        //     bool hasData = entry.Data != null;
        //     LoadStarted?.Invoke(name, hasData);
        //     AsyncOperation sceneLoad = SceneManager.LoadSceneAsync(entry.Index,
        //         isAdditive ? LoadSceneMode.Additive : LoadSceneMode.Single);
        //     sceneLoad.allowSceneActivation = false;
        //     if (loadingScreen != null)
        //     {
        //         loadingScreen.allowSceneActivation = true;
        //         await Await.Until(() => loadingScreen.progress >= 1.0f);
        //     }
        //
        //     SceneManager.sceneLoaded += OnSceneLoad;
        //
        //     while (sceneLoad.progress < 0.9f || minLoadTimeMS > loadTimer.ElapsedMilliseconds)
        //     {
        //         await Await.NextUpdate();
        //     }
        //
        //     loadTimer.Stop();
        //     sceneLoad.allowSceneActivation = true;
        //     GameManager.CurrentLevelData = entry.Data;
        //     GameManager.CurrentLevelData.CurrencyEarned = 0;
        //
        //     // await Await.Until(() => sceneLoad.progress >= 1.0f);
        //     await sceneLoadEvent.WaitAsync();
        //     SceneManager.sceneLoaded -= OnSceneLoad;
        //     LoadFinished?.Invoke(name, hasData);
        // }

        protected override Task OnInitialise()
        {
            return Task.CompletedTask;
        }

        protected override void Reset()
        { }
    }
}