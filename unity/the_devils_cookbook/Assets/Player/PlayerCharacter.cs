using FMODUnity;
using Nito.AsyncEx;
using System;
using System.Threading.Tasks;
using TDC.Core.Manager;
using TDC.Core.Utility;
using TDC.Interactions;
using TDC.Items;
using TDC.Spellcasting;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TDC.Player
{
    [RequireComponent(typeof(PlayerMovement), typeof(Interactor), typeof(Spellcaster))]
    [RequireComponent(typeof(Inventory))]
    public class PlayerCharacter : MonoBehaviour
    {
        [SerializeField][SerializedValueRequired] private PlayerStats _PlayerStats;
        [SerializeField][SerializedValueRequired] public PlayerMovement Movement;
        [SerializeField][SerializedValueRequired] public Interactor Interactor;
        [SerializeField][SerializedValueRequired] public Spellcaster Spellcaster;
        [SerializeField][SerializedValueRequired] public Inventory Inventory;

        public Animator Animator;

        public EventReference DashAudioEvent;

        private Action<InputAction.CallbackContext> _CastSpell1;
        private Action<InputAction.CallbackContext> _CastSpell2;
        private Action<InputAction.CallbackContext> _CastSpell3;
        private Action<InputAction.CallbackContext> _CastSpell4;
        private Action<InputAction.CallbackContext> _Interact;
        private Action<InputAction.CallbackContext> _Cancel;

        private void CancelCast(InputAction.CallbackContext _) => Spellcaster.CancelCast();

        private AsyncManualResetEvent _StatsInitialized = new AsyncManualResetEvent();

        private string SaveData;

        private Func<bool, Task> _OnLevelEnd;

        protected async void Awake()
        {
            Movement ??= GetComponent<PlayerMovement>();
            Interactor ??= GetComponent<Interactor>();
            Spellcaster ??= GetComponent<Spellcaster>();
            Inventory ??= GetComponent<Inventory>();

            SaveData = Application.persistentDataPath + "/player.dat";

            this.Validate();

            _PlayerStats = (await _PlayerStats.Load(SaveData));
            _PlayerStats = Instantiate(_PlayerStats);
            DontDestroyOnLoad(_PlayerStats);
            await _PlayerStats.Initialize();
            _StatsInitialized.Set();
        }

        public async void Start()
        {
            _CastSpell1 = context => Spellcaster.TryCast(0);
            _CastSpell2 = context => Spellcaster.TryCast(1);
            _CastSpell3 = context => Spellcaster.TryCast(2);
            _CastSpell4 = context => Spellcaster.TryCast(3);
            _Interact = context => Interactor.Interact(Interactor.GetInteractions());
            _Cancel = context => Interactor.Interact(Interaction.Deactivate);

            GameManager.RunOnInitialisation(RegisterInput);
            GameManager.OnLevelEnd += _OnLevelEnd;

            (await GetPlayerStats()).Health.OnValueSet += (health) => { if (health == 0) { Movement.AnimatoionController.SetTrigger("Die"); } };
        }

        private void RegisterInput()
        {
            GameManager.PlayerControls.Player.Spell0.performed += _CastSpell1;
            GameManager.PlayerControls.Player.Spell1.performed += _CastSpell2;
            GameManager.PlayerControls.Player.Spell2.performed += _CastSpell3;
            GameManager.PlayerControls.Player.Spell3.performed += _CastSpell4;

            GameManager.PlayerControls.Player.Cancel.performed += CancelCast;

            GameManager.PlayerControls.Player.Interact.performed += _Interact;
            GameManager.PlayerControls.Player.Menu.performed += _Cancel;
        }

        public void OnDestroy()
        {
            GameManager.PlayerControls.Player.Spell0.performed -= _CastSpell1;
            GameManager.PlayerControls.Player.Spell1.performed -= _CastSpell2;
            GameManager.PlayerControls.Player.Spell2.performed -= _CastSpell3;
            GameManager.PlayerControls.Player.Spell3.performed -= _CastSpell4;

            GameManager.PlayerControls.Player.Cancel.performed -= CancelCast;

            GameManager.PlayerControls.Player.Interact.performed -= _Interact;
            GameManager.PlayerControls.Player.Menu.performed -= _Cancel;
            GameManager.OnLevelEnd -= _OnLevelEnd;

            _PlayerStats.Save(SaveData, true);
        }

        public void Interact()
        {
            _Interact?.Invoke(new InputAction.CallbackContext());
        }

        public InputAction GetSpellInputAction(int index)
        {
            return GameManager.PlayerControls.FindAction($"Spell{index}");
        }

        public async Task<PlayerStats> GetPlayerStats()
        {
            await _StatsInitialized.WaitAsync();
            return _PlayerStats;
        }

        private void OnEnable()
        {
            // GameManager.PlayerControls.Enable();
        }

        // private void OnDisable()
        // {
        //     GameManager.PlayerControls.Disable();
        // }
    }
}