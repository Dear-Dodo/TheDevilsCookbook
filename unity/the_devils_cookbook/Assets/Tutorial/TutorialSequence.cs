using Nito.AsyncEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using TDC.AIRefac;
using TDC.Cooking;
using TDC.Core.Extension;
using TDC.Core.Manager;
using TDC.Core.Utility;
using TDC.Ingredient;
using TDC.Items;
using TDC.Ordering;
using TDC.Patrons;
using TDC.Player;
using TDC.Spellcasting;
using TDC.UI;
using TDC.UI.Dialogue;
using TDC.UI.Objectives;
using UnityAsync;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TDC.Tutorial
{
    public class TutorialSequence : Sequence
    {
        public float TutorialStepDelay = 1.5f;

        [Header("Dialogue")]
        [SerializeField] private DialogueData _MovementDialogue;

        [SerializeField] private DialogueData _TitaniaDialogue;
        [SerializeField] private Food _TitaniaFoodTarget;

        [SerializeField] private DialogueData _TrapDialogue;
        [SerializeField] private Spell _TrapSpell;

        [SerializeField] private DialogueData _SpellsDialogue;
        [SerializeField] private Spell _WallSpell;
        [SerializeField] private Spell _PullSpell;
        [SerializeField] private Spell _PushSpell;

        [SerializeField] private DialogueData _CookDialogue;
        [SerializeField] private Food _CookFoodTarget;

        [SerializeField] private DialogueData _FailDialogue1;
        [SerializeField] private Recipe _FailRecipe;
        [SerializeField] private PatronWindow _FailWindow;
        [SerializeField] private float _FailPatronTime = 5.0f;
        [SerializeField] private DialogueData _FailDialogue2;

        [SerializeField] private DialogueData _BinDialogue;

        [SerializeField] private DialogueData _ServeDialogue;
        [SerializeField] private Recipe _ServeRecipe;

        [SerializeField] private DialogueData _PlayDialogue;
        [SerializeField] private int _PlayPatronServeCount = 3;
        [SerializeField] private DialogueData _SavedFromDeathDialogue;

        [SerializeField] private DialogueData _EndDialogue;

        [SerializeField] private LoseScreen _LoseScreenReference;
        [SerializeField] private WinScreen _WinScreenReference;

        [Header("Triggers")]
        [SerializeField] private ColliderProxy _DashProxy;

        [SerializeField] private ColliderProxy _ImbuerRoomProxy;

        private ObjectiveManager _ObjectiveManager;
        private PlayerCharacter _Player;

        private readonly AsyncAutoResetEvent ObjectivesCompletedAsync = new AsyncAutoResetEvent();

        public void SetupObjective<T>(object eventContainer, string eventReflectionName, T listener, Objective objective,
            bool isStatic = false) where T : Delegate
        {
            BindingFlags flags = isStatic ? BindingFlags.Static | BindingFlags.Public
                : BindingFlags.Public | BindingFlags.Instance;
            EventInfo progressEvent = eventContainer.GetType().GetEvent(eventReflectionName, flags);
            progressEvent.AddEventHandler(eventContainer, listener);
            objective.Completed += (_) => progressEvent.RemoveEventHandler(eventContainer, listener);
            _ObjectiveManager.AddObjective(objective);
        }

        private void Awake()
        {
            _ObjectiveManager = GetComponent<ObjectiveManager>();
        }

        private void OnObjectivesCompleted()
        {
            ObjectivesCompletedAsync.Set();
        }

        private async Task DoMoveStep()
        {
            var moveObj = new BooleanObjective("Move with the 'W', 'A', 'S', and 'D' keys.");
            SetupObjective(GameManager.PlayerControls.Player.Move, nameof(GameManager.PlayerControls.Player.Move.performed),
                new Action<InputAction.CallbackContext>(_ => moveObj.Complete()), moveObj);

            var dashObj = new BooleanObjective("Dash with the 'Shift' key.");
            SetupObjective(GameManager.PlayerControls.Player.Run, nameof(GameManager.PlayerControls.Player.Run.performed),
                new Action<InputAction.CallbackContext>(_ => dashObj.Complete()), dashObj);

            var dashObstaclesObj = new BooleanObjective("Dash to the other side of the obstacles.");
            SetupObjective<Action<Collider>>(_DashProxy, nameof(_DashProxy.TriggerEntered),
                _ => dashObstaclesObj.Complete(), dashObstaclesObj);

            var imbuerRoomObj = new BooleanObjective("Enter the next room.");
            SetupObjective<Action<Collider>>(_ImbuerRoomProxy, nameof(_ImbuerRoomProxy.TriggerEntered),
                _ => imbuerRoomObj.Complete(), imbuerRoomObj);

            await GameManager.DialogueSystem.Run(_MovementDialogue);
            await ObjectivesCompletedAsync.WaitAsync();
            _ObjectiveManager.ClearObjectives();
        }

        private async Task<List<Creature>> DoSummonStep()
        {
            var summonObj = new BooleanObjective("Summon the ingredients for a bad kebab.");
            var spawned = new List<Creature>();
            void CheckCompleteSummon(Recipe recipe, List<Creature> creatures)
            {
                if (recipe.Output[0].Name != _TitaniaFoodTarget.Name) return;
                summonObj.Complete();
                spawned = creatures;
            }

            var imbuer = FindObjectOfType<SoulImbuer>(true);
            SetupObjective<Action<Recipe, List<Creature>>>
                (imbuer, nameof(imbuer.OnSpawnRecipe), CheckCompleteSummon, summonObj);

            GameManager.PlayerControls.Player.Interact.Enable();
            await GameManager.DialogueSystem.Run(_TitaniaDialogue);
            await ObjectivesCompletedAsync.WaitAsync();
            _ObjectiveManager.ClearObjectives();
            return spawned;
        }

        private async Task DoCatchStep(List<Creature> creatures)
        {
            _Player.Spellcaster.AddSpell(_TrapSpell);
            foreach (Creature target in creatures)
            {
                var objective = new BooleanObjective($"Catch the {target.ContainedFood.Name}.");
                target.OnCaught += () => objective.Complete();
                _ObjectiveManager.AddObjective(objective);
            }

            GameManager.PlayerControls.Player.Spell0.Enable();
            GameManager.PlayerControls.Player.Fire.Enable();
            GameManager.PlayerControls.Player.Cancel.Enable();
            await GameManager.DialogueSystem.Run(_TrapDialogue);
            await ObjectivesCompletedAsync.WaitAsync();
            _ObjectiveManager.ClearObjectives();
        }

        private void SetupSpellObjective(SpellData spell, int spellIndex, string objectiveText, out string controlName, bool requireHits = true)
        {
            InputAction control = _Player.GetSpellInputAction(spellIndex);
            control.Enable();
            controlName = control.controls[0].name.ToUpper();
            var objective = new BooleanObjective(objectiveText.Replace("[KEYBIND]", controlName));

            if (requireHits)
            {
                void Complete(IEnumerable<Agent> agents)
                {
                    if (!agents.Any()) return;
                    objective.Complete();
                }
                SetupObjective<Action<IEnumerable<Agent>>>(spell, nameof(spell.SpellTickPerformed), Complete, objective);
            }
            else
            {
                void Complete(bool successful)
                {
                    if (!successful) return;
                    objective.Complete();
                }
                SetupObjective<Action<bool>>(spell, nameof(spell.CastAttempted), Complete, objective);
            }
        }

        private async Task DoSpellsStep()
        {
            _Player.Spellcaster.AddSpell(_WallSpell);
            _Player.Spellcaster.AddSpell(_PullSpell);
            _Player.Spellcaster.AddSpell(_PushSpell);
            SpellData[] spells = _Player.Spellcaster.Spells;

            SetupSpellObjective(spells[1], 1, "Cast the wall spell (Key: [KEYBIND])",
                out string wallControl, false);
            SetupSpellObjective(spells[2], 2, $"Pull an ingredient with {_PullSpell.DisplayName} (Key: [KEYBIND])",
                out string pullControl);
            SetupSpellObjective(spells[3], 3, $"Push an ingredient with {_PushSpell.DisplayName} (Key: [KEYBIND])",
                out string pushControl);

            string[] controls = { wallControl, pullControl, pushControl };
            DialogueData parsedData = _SpellsDialogue.Clone();
            var controlIndex = 0;
            foreach (DialogueData.Message t in parsedData.Messages)
            {
                if (!t.Text.Contains("[KEYBIND]")) continue;
                t.Text = t.Text.Replace("[KEYBIND]", controls[controlIndex]);
                controlIndex++;
            }

            await GameManager.DialogueSystem.Run(parsedData);
            await ObjectivesCompletedAsync.WaitAsync();
            _ObjectiveManager.ClearObjectives();
        }

        private async Task DoCookingStep()
        {
            var oven = FindObjectOfType<Oven>();
            var startCookObj = new BooleanObjective($"Put the ingredients for a {_CookFoodTarget.Name} in the oven.");
            void CompleteStart(Food food)
            {
                if (food.Name != _CookFoodTarget.Name) return;
                startCookObj.Complete();
            }

            SetupObjective<Action<Food>>(oven, nameof(oven.CookingStarted), CompleteStart, startCookObj);

            var finishCookObj = new BooleanObjective("Wait for the food to cook.");
            void CompleteCook(Food food)
            {
                finishCookObj.Complete();
            }
            SetupObjective<Action<Food>>(oven, nameof(oven.CookingFinished), CompleteCook, finishCookObj);

            var withdrawObj = new BooleanObjective("Pick up the finished dish.");
            void CompleteWithdraw(Food food)
            {
                withdrawObj.Complete();
            }

            SetupObjective<Action<Food>>(oven, nameof(oven.FoodWithdrawn), CompleteWithdraw, withdrawObj);

            await GameManager.DialogueSystem.Run(_CookDialogue);
            await ObjectivesCompletedAsync.WaitAsync();
            _ObjectiveManager.ClearObjectives();
        }

        private async Task DoFailStep()
        {
            await GameManager.DialogueSystem.Run(_FailDialogue1);
            Order order = await GameManager.OrderManager.CreateNextOrder(_FailRecipe, _FailWindow);
            Patron patron = order.Patron;
            order.IsPaused = true;
            order.Time = _FailPatronTime;
            await patron.FinishedEntranceAsync.WaitAsync();
            await GameManager.DialogueSystem.Run(_FailDialogue2);
            order.IsPaused = false;
            await order.CompletedAsync.WaitAsync();
            await patron.ExitedMapAsync.WaitAsync();
        }

        private async Task DoBinStep()
        {
            var objective = new BooleanObjective("Throw the kebab into Toaby's mouth. (Interact with Toaby and click in your inventory)");
            void Complete(StorableObject removed)
            {
                if (removed.Name == _CookFoodTarget.Name) objective.Complete();
            }

            SetupObjective<Action<StorableObject>>
                (_Player.Inventory, nameof(_Player.Inventory.ItemRemoved), Complete, objective);

            await GameManager.DialogueSystem.Run(_BinDialogue);
            await ObjectivesCompletedAsync.WaitAsync();
            _ObjectiveManager.ClearObjectives();
        }

        private async Task DoServeStep()
        {
            Food targetFood = _ServeRecipe.Output[0];
            foreach (Food food in _ServeRecipe.Input)
            {
                var objective = new BooleanObjective($"Collect ingredient: {food.Name}");
                _ObjectiveManager.AddObjective(objective);
                if (_Player.Inventory.HasItem(0, food))
                {
                    objective.Complete(false);
                    continue;
                }

                void Complete(StorableObject item)
                {
                    if (item.Name == food.Name || item.Name == _ServeRecipe.Output[0].Name) objective.Complete();
                }
                _Player.Inventory.ItemAdded += Complete;
                objective.Completed += (_) => _Player.Inventory.ItemAdded -= Complete;
            }

            var cookObjective = new BooleanObjective($"Cook and collect dish: {targetFood.Name}");
            _ObjectiveManager.AddObjective(cookObjective);
            if (_Player.Inventory.HasItem(1, targetFood)) cookObjective.Complete();
            else
            {
                void Complete(StorableObject item)
                {
                    if (item.Name == targetFood.Name) cookObjective.Complete(false);
                }

                _Player.Inventory.ItemAdded += Complete;
                cookObjective.Completed += (_) => _Player.Inventory.ItemAdded -= Complete;
            }

            var serveObjective = new BooleanObjective($"Serve the patron their food.");
            _ObjectiveManager.AddObjective(serveObjective);
            void CompleteServe(bool _) => serveObjective.Complete();

            await GameManager.DialogueSystem.Run(_ServeDialogue);

            Order order = await GameManager.OrderManager.CreateNextOrder(_ServeRecipe, _FailWindow);
            order.IsPaused = true;

            order.OnCompleted += CompleteServe;
            serveObjective.Completed += (_) => order.OnCompleted -= CompleteServe;

            await ObjectivesCompletedAsync.WaitAsync();
            _ObjectiveManager.ClearObjectives();
        }

        private async Task DoPlayStep()
        {
            var hasPlayedSaveDialogue = false;
            int ReduceLethalDamage(PlayerStats stats, int value)
            {
                if (!hasPlayedSaveDialogue && stats.Health == 1)
                {
                    _ = GameManager.DialogueSystem.Run(_SavedFromDeathDialogue);
                    hasPlayedSaveDialogue = true;
                }
                return stats.Health == 1 ? 0 : value;
            }

            PlayerStats stats = await GameManager.PlayerCharacter.GetPlayerStats();
            stats.PlayerDamageModifiers += ReduceLethalDamage;

            var objective = new QuantifiableObjective("Serve patrons before they lose patience.", 3, 0);
            void ProgressObjective(Order order, bool b)
            {
                if (b) objective.Increment();
            }
            SetupObjective<Action<Order, bool>>(GameManager.OrderManager, nameof(GameManager.OrderManager.OnOrderCompleted),
                ProgressObjective, objective);

            await GameManager.DialogueSystem.Run(_PlayDialogue);
            _ = GameManager.OrderManager.Begin();

            await ObjectivesCompletedAsync.WaitAsync();
            _ObjectiveManager.ClearObjectives();
            GameManager.OrderManager.Cancel();
            GameManager.OrderManager.CompleteAllOrders();
            stats.PlayerDamageModifiers -= ReduceLethalDamage;
        }

        public override async Task Run()
        {
            try
            {
                _Player = FindObjectOfType<PlayerCharacter>();
                _Player.Spellcaster.ClearSpells();
                await GameManager.LevelIntroCutscene.OnComplete.WaitAsync();
                await _ObjectiveManager.EnableObjectives();
                _ObjectiveManager.ObjectivesCompleted += OnObjectivesCompleted;

                GameManager.PlayerControls.Player.Disable();
                GameManager.PlayerControls.Player.Move.Enable();
                GameManager.PlayerControls.Player.Run.Enable();
                GameManager.PlayerControls.Player.Pause.Enable();

                await DoMoveStep();
                await Await.Seconds(TutorialStepDelay);

                List<Creature> spawned = await DoSummonStep();
                await Await.Seconds(TutorialStepDelay);

                await DoCatchStep(spawned);
                await Await.Seconds(TutorialStepDelay);

                await DoSpellsStep();
                await Await.Seconds(TutorialStepDelay);

                await DoCookingStep();
                await Await.Seconds(TutorialStepDelay);

                await DoFailStep();
                await Await.Seconds(TutorialStepDelay);

                await DoBinStep();
                await Await.Seconds(TutorialStepDelay);

                await DoServeStep();
                await Await.Seconds(TutorialStepDelay);

                await DoPlayStep();
                await Await.Seconds(TutorialStepDelay);

                _ObjectiveManager.DisableObjectives();

                await GameManager.DialogueSystem.Run(_EndDialogue);

                _LoseScreenReference.RetryButton.interactable = false;
                _WinScreenReference.NextButton.interactable = false;

                GameManager.OrderSystemComplete.SetResult(true);
                await GameManager.EndLevel();
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                throw;
            }
            finally
            {
                _ObjectiveManager.ObjectivesCompleted -= OnObjectivesCompleted;
            }
        }
    }
}