using DG.Tweening;
using Nito.AsyncEx;
using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using TDC.Cooking;
using TDC.Core.Extension;
using TDC.Core.Type;
using TDC.Debugging;
using TDC.Input;
using TDC.Level;
using TDC.Ordering;
using TDC.Patrons;
using TDC.Player;
using TDC.UI;
using TDC.UI.Dialogue;
using TDC.UI.Windowing;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Random = System.Random;

namespace TDC.Core.Manager
{
    [AddressablePrefab("Game Manager"), Persistent, SingletonInitializeOnRuntime]
    public class GameManager : Type.Singleton<GameManager>
    {
        private static MethodInfo _ForceExecuteUnitySync;

        public delegate Task AsyncEventHandler<in TEventArgs>(object sender, TEventArgs args);

        /// <summary>
        /// Reference to PlayerCharacter
        /// </summary>
        public static PlayerCharacter PlayerCharacter { get; private set; }

        public static Appliance[] Appliances { get; private set; }

        public static PlayerControls PlayerControls { get; private set; }

        /// <summary>
        /// Reference to SceneLoader
        /// </summary>
        public static SceneLoader SceneLoader => Instance._SceneLoader;

        /// <summary>
        /// Reference to WindowManager
        /// </summary>
        public static WindowManager WindowManager => Instance._WindowManager;

        /// <summary>
        /// Reference to PatronManager
        /// </summary>
        public static PatronManager PatronManager => Instance._PatronManager;

        /// <summary>
        /// Reference to OrderManager
        /// </summary>
        public static OrderManager OrderManager => Instance._OrderManager;

        /// <summary>
        /// Reference to AudioManager
        /// </summary>
        public static AudioManager AudioManager => Instance._AudioManager;

        public static UserSettings UserSettings => Instance._UserSettings;
        public static CreatureManager CreatureManager => Instance._CreatureManager;

        public static LevelLoader LevelLoader => Instance._LevelLoader;

        public static DialogueSystem DialogueSystem => Instance._DialogueSystem;

        public static LevelIntroCutscene LevelIntroCutscene => Instance._LevelIntroCutscene;

        public static UIHider UIHider => Instance._UIHider;

        /// <summary>
        /// Can be null.
        /// </summary>
        public static LevelData CurrentLevelData;

        public static Random GameRandom;

        /// <summary>
        /// Invoked when a game level has finished loading and initialising.
        /// </summary>
        public static event Action LevelInitialised;
        
        /// <summary>
        /// Invoked when a game level has finished loading and initialising.
        /// Does not clear after the level is loaded
        /// </summary>
        public static event Action LevelInitialisedPersistant;

        public static AsyncManualResetEvent LevelInitialisedAsync = new AsyncManualResetEvent();

        public static event Action Initialised;

        public static AsyncManualResetEvent InitialisedAsync = new AsyncManualResetEvent(false);

        public static AsyncManualResetEvent PlayerDataWriteAsync = new AsyncManualResetEvent(true);

        /// <summary>
        /// Invoked when all systems have been fully initialised and loaded.
        /// </summary>
        public static event Action GameFullyLoaded;

        /// <summary>
        /// Set on scene load, indicates the GM has attempted to get the player.
        /// </summary>
        public static AsyncManualResetEvent PlayerInitialised = new AsyncManualResetEvent();

        private static bool _IsLoadingLevel;

        // TODO: Appropriately place these events in the GameManager class at appropriate times.

        public static event Func<bool, Task> OnLevelEnd;

        public static Action<int> OnHealthSet
        { get { return Instance._OnHealthSet; } set { Instance._OnHealthSet = value; } }

        public static int CurrentLevel
        { get { return Instance._CurrentLevel; } set { Instance._CurrentLevel = value; } }

        public static float Timescale
        { get { return Instance._Timescale; } set { Instance._Timescale = value; } }

        public static TaskCompletionSource<bool> OrderSystemComplete
        { get { return Instance._OrderSystemComplete; } set { Instance._OrderSystemComplete = value; } }

        public static bool LevelRunning
        { get { return Instance._LevelRunning; } set { Instance._LevelRunning = value; } }

        private bool _LevelRunning = false;

        public static float TipPecrcent
        { get { return Instance._TipPecrcent; } set { Instance._TipPecrcent = value; } }

        [SerializeField]
        private float _TipPecrcent = 0.25f;

        [SerializeField] private WindowManager _WindowManager;
        [SerializeField] private PatronManager _PatronManager;
        [SerializeField] private OrderManager _OrderManager;
        [SerializeField] private AudioManager _AudioManager;
        [SerializeField] private SceneLoader _SceneLoader;
        [SerializeField] private LevelLoader _LevelLoader;
        [SerializeField] private DialogueSystem _DialogueSystem;
        [SerializeField] private DebugConsole _DebugConsole;
        [SerializeField] private LevelIntroCutscene _LevelIntroCutscene;
        private readonly UIHider _UIHider = new UIHider();

        private readonly UserSettings _UserSettings = new UserSettings();
        private readonly CreatureManager _CreatureManager = new CreatureManager();

        private int _CurrentLevel;
        private float _Timescale = 1;

        private Action<int> _OnHealthSet;
        private TaskCompletionSource<bool> _OrderSystemComplete = new TaskCompletionSource<bool>();

        /// <summary>
        /// Runs <paramref name="onInitialised"/> when initialisation completes, or immediately if it has.
        /// </summary>
        /// <param name="onInitialised"></param>
        public static void RunOnInitialisation(Action onInitialised)
        {
            if (InitialisedAsync.IsSet)
            {
                onInitialised();
            }
            else Initialised += onInitialised;
        }

        /// <summary>
        /// Runs <paramref name="onLevelInitialised"/> when initialisation completes, or immediately if it has.
        /// </summary>
        /// <param name="onLevelInitialised"></param>
        public static void RunOnLevelInitialisation(Action onLevelInitialised)
        {
            if (LevelInitialisedAsync.IsSet)
            {
                onLevelInitialised();
            }
            else LevelInitialised += onLevelInitialised;
        }

        /// <summary>
        /// Runs <paramref name="onLevelInitialised"/> when initialisation completes, or immediately if it has.
        /// </summary>
        /// <param name="onLevelInitialised"></param>
        public static void RunOnLevelInitialisationPersistant(Action onLevelInitialised)
        {
            if (LevelInitialisedAsync.IsSet)
            {
                onLevelInitialised();
            }
            else LevelInitialisedPersistant += onLevelInitialised;
        }

        public async void Update()
        {
            Time.timeScale = _Timescale;
            await _OrderManager.Update();

            //GameOver logic
            if (CurrentLevelData != null && CurrentLevelData.OrderCount != -1 &&  LevelRunning && OrderManager.RemainingOrders <= 0)
            {
                await EndLevel();
            }
        }

        public static async void InitialiseLevel()
        {
            PlayerCharacter = FindObjectOfType<PlayerCharacter>(true);
            Appliances = FindObjectsOfType<Appliance>(true);

            PlayerControls.Enable();
            PlayerInitialised.Set();

            // Force unity to evaluate async continuations immediately to avoid Update() being called before continuations.
            _ForceExecuteUnitySync.Invoke(SynchronizationContext.Current, null);

            // TODO: Register OrderManager to LevelStart.

            GameRandom = new Random(CurrentLevelData.LevelSeed);
            CurrentLevelData.CurrencyEarned = 0;

            PatronManager.OnAllPatronsRemoved += CompleteOrderSystem;

            //GameOver logic
            OnHealthSet = async (health) =>
            {
                AudioManager.SetGlobalParameter("Health", health);
                if (health <= 0)
                {
                    AudioManager.SetParameterOverTime("LevelMusic", "Cutoff", 0, 2.5f);
                    await EndLevel();
                }
            };
            AudioManager.SetGlobalParameter("Health", 3);
            AudioManager.SetParameterOverTime("MenuMusic", "fade", 0, 2.5f);
            if (CurrentLevel == 0)
            {
                AudioManager.PlaySound("TutorialMusic");
                AudioManager.SetParameter("TutorialMusic", "Fade", 1);
            }
            else
            {
                AudioManager.PlaySound("LevelMusic");
                AudioManager.SetParameter("LevelMusic", "Fade", 1);
                AudioManager.SetParameter("LevelMusic", "Cutoff", 1);
            }

            (await PlayerCharacter.GetPlayerStats()).Health.OnValueSet += OnHealthSet;
            _IsLoadingLevel = false;
            LevelInitialised?.Invoke();
            LevelInitialisedPersistant?.Invoke();
            LevelInitialised = null;
            LevelInitialisedAsync.Set();
            var sequence = FindObjectOfType<TDC.Tutorial.Sequence>();
            if (sequence) _ = sequence.Run();
            LevelRunning = true;
        }

        public static async Task StartLevel(int number)
        {
            _IsLoadingLevel = true;
            await LevelLoader.LoadLevel(number);
            CurrentLevel = number;
            InitialiseLevel();

            if (LevelLoader.FadeToBlackObject)
                Destroy(LevelLoader.FadeToBlackObject);
            LevelLoader.FadeToBlackObject = Instantiate(LevelLoader.FadeToBlackPrefab, new Vector3(), Quaternion.identity);
            LevelLoader.FadeToBlackObject.gameObject.SetActive(true);
            LevelLoader.FadeToBlackObject.Image.color = new Color(0, 0, 0, 1);
            await LevelLoader.FadeToBlackObject.Image.DOColor(new Color(0, 0, 0, 0), LevelLoader.FadeToBlackTime).AsyncWaitForCompletion();
            LevelLoader.FadeToBlackObject.gameObject.SetActive(false);
        }

#if UNITY_EDITOR

        private void _InitialiseForStartingScene()
        {
            string scenePath = SceneManager.GetActiveScene().path;
            string assetPath = Path.Combine(
                Application.dataPath.Substring(0, Application.dataPath.LastIndexOf("/Assets")), scenePath);
            if (!Directory.Exists(Path.GetDirectoryName(assetPath)))
            {
                Debug.Log($"Scene {assetPath} was not an asset. Skipping level initialisation.");
                return;
            }
            string sceneDirectory = Path.GetDirectoryName(scenePath);
            string[] dataGUIDs = UnityEditor.AssetDatabase.FindAssets("t:LevelData", new[] { sceneDirectory });
            if (dataGUIDs.Length == 0)
            {
                Debug.Log($"No level data found for {scenePath}, will not initialise as level.");
                return;
            }

            string dataPath = UnityEditor.AssetDatabase.GUIDToAssetPath(dataGUIDs[0]);
            if (dataGUIDs.Length > 1)
            {
                Debug.Log($"{dataGUIDs.Length} LevelData objects found for {scenePath}. Assuming first data and " +
                          $"initialising as level. ({dataPath})");
            }
            var data = UnityEditor.AssetDatabase.LoadAssetAtPath<LevelData>(dataPath);
            int levelIndex = LevelLoader.GetLevels().FirstIndexOf((entry) => entry.Data == data);
            if (levelIndex < 0)
            {
                Debug.LogError($"Unable to find index of LevelData for {scenePath} ({data}) in LevelLoader.");
                return;
            }
            CurrentLevel = levelIndex;
            CurrentLevelData = data;
            InitialiseLevel();

            PlayerControls.Player.Pause.performed += OnPause;
        }

#endif

        public override void OnInitialize()
        {
            _ForceExecuteUnitySync = SynchronizationContext.Current.GetType()
                .GetMethod("Exec", BindingFlags.NonPublic | BindingFlags.Instance);
            PlayerControls ??= new PlayerControls();
            PlayerControls.Enable();

            SceneLoader.OnSceneLoadStarted += OnStartLoad;
            LevelLoader.onLevelLoadFinished += OnEndLoadLevel;

            _SceneLoader.Initialise();
            _PatronManager.Initialise();
            _WindowManager.Initialise();
            _OrderManager.Initialise();
            _UserSettings.Initialise();
            _CreatureManager.Initialise();
            _LevelIntroCutscene.Initialise();
            _UIHider.Initialise();
            _AudioManager = GetComponentInChildren<AudioManager>();
            _DebugConsole = Instantiate(_DebugConsole);

            InitialisedAsync.Set();
            Initialised?.Invoke();

#if UNITY_EDITOR
            _InitialiseForStartingScene();
#endif
        }

        public static async Task EndLevel()
        {
            if (LevelRunning)
            {
                LevelRunning = false;
                PlayerInitialised.Reset();
                PlayerControls.Player.Disable();
                AudioManager.SetParameterOverTime("LevelMusic", "Fade", 0, 2.5f);
                AudioManager.SetParameterOverTime("TutorialMusic", "Fade", 0, 2.5f);

                await OrderSystemComplete.Task;
                bool win = (await PlayerCharacter.GetPlayerStats()).Health > 0;

                await (OnLevelEnd?.Invoke(win) ?? Task.CompletedTask);

                OrderSystemComplete = new TaskCompletionSource<bool>();
            }
        }

        public static Task CachePlayer()
        {
            PlayerCharacter = FindObjectOfType<PlayerCharacter>(true);
            PlayerInitialised.Set();
            return Task.CompletedTask;
        }

        // TODO: Move to a PauseManager script

        private static WindowNode _PauseWindowInstance;

        private static async void OnPause(InputAction.CallbackContext context)
        {
            if (_PauseWindowInstance == null)
            {
                _PauseWindowInstance = await WindowManager.OpenAdditive("Pause");
            }
            else
            {
                if (await _PauseWindowInstance.Close(false))
                {
                    _PauseWindowInstance = null;
                }
            }
        }

        private static void OnStartLoad(SceneEntry name)
        {
            PlayerInitialised.Reset();
            PlayerControls.Player.Pause.performed -= OnPause;
            _PauseWindowInstance = null;
            LevelInitialisedAsync.Reset();
        }

        private static void OnEndLoadLevel(LevelEntry entry)
        {
            PlayerControls.Player.Pause.performed += OnPause;
        }

        private static void CompleteOrderSystem()
        {
            OrderSystemComplete.SetResult(true);
        }
    }
}