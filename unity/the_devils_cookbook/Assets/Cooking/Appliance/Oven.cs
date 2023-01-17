using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TDC.Core.Manager;
using TDC.Core.Utility;
using TDC.Interactions;
using TDC.Items;
using TDC.Procedural;
using TDC.UI.Generic;
using TDC.UI.Menu;
using UnityAsync;
using UnityEngine;

namespace TDC.Cooking
{
    public class Oven : Appliance
    {
        public RadialMenu RadialMenuPrefab;
        public RadialMenu RadialSubMenuPrefab;
        public Canvas HUD;
        public TimerUI Timer;
        public Texture2D Overlay;

        public GameObject SpawnedFood;
        public GameObject SpawnFoodLocation;

        public EyeballController EyeController;

        public AnimationClip OvenOpenAnimation;

        [SerializeField, SerializedValueRequired] 
        private Collider _OpenInteractTrigger;

        public Action OnOpenRadialMenu;
        public Action OnCloseRadialMenu;

        public event Action<Food> CookingStarted;

        public event Action<Food> CookingFinished;

        public event Action<Food> FoodWithdrawn;

        public EventReference OpenSFX;
        public EventReference CloseSFX;
        public EventReference SizzleSFX;
        public EventReference AmbientSFX;
        public EventReference ChimeSFX;

        private EventInstance _AmbientSFX;

        private Interactor _CurrentInteractor;
        private RadialMenu _Menu;
        private Recipe _CurrentRecipe;

        private readonly Dictionary<RadialMenuSlot, RadialMenu> _SubMenus = new Dictionary<RadialMenuSlot, RadialMenu>();
        private UIOverlay _Overlay;

        public override Interaction GetInteractions(Interactor interactor)
        {
            Interaction interaction = Interaction.None;
            switch (State)
            {
                case ProcessState.Inactive:
                    interaction = Interaction.None;
                    break;

                case ProcessState.Ready:
                    interaction = Interaction.Deposit;
                    break;

                case ProcessState.Active:
                    interaction = Interaction.None;
                    break;

                case ProcessState.Complete:
                    interaction = SpawnedFood ? Interaction.Withdraw : Interaction.None;
                    break;
            }
            return interaction;
        }

        public override void OnHover(Interactor interactor)
        {
            base.OnHover(interactor);
            EyeController.TargetObject = interactor.gameObject;
        }

        public override void ExitHover(Interactor interactor)
        {
            base.ExitHover(interactor);
            HideRadialMenu();
            EyeController.TargetObject = EyeController.DefaultTarget;
        }

        public override void Interact(Interactor interactor, Interaction interaction)
        {
            switch (interaction)
            {
                case Interaction.Deactivate:
                    HideRadialMenu();
                    break;

                case Interaction.Deposit:
                    _CurrentInteractor = interactor;
                    ShowRadialMenu();
                    break;

                case Interaction.Inspect:
                    break;

                case Interaction.Withdraw:
                    if (SpawnedFood == null)
                    {
                        ShowPrompt = false;
                        return;
                    }
                    Dictionary<StorableObject, int> remainingItems = new Dictionary<StorableObject, int>();
                    if (Inventory.HasItems(Query.MakeQuery(_CurrentRecipe.Output)))
                    {
                        Inventory.RemoveItems(Query.MakeQuery(_CurrentRecipe.Output));
                        if (!interactor.Inventory.DepositItems(Item.ToItems(_CurrentRecipe.Output), ItemTypes.OrderableFood, out remainingItems))
                        {
                            Inventory.DepositItems(remainingItems, ItemTypes.OrderableFood);
                        }
                        else
                        {
                            FoodWithdrawn?.Invoke(_CurrentRecipe.Output[0]);
                            _CurrentRecipe = null;
                            State = ProcessState.Ready;
                        }
                    }

                    Animator.SetTrigger("OvenClose");
                    _OpenInteractTrigger.enabled = false;
                    if (SpawnedFood)
                    {
                        Destroy(SpawnedFood);
                    }
                    break;
            }
        }

        private void ShowRadialMenu()
        {
            foreach (RadialMenu oldMenu in HUD.GetComponentsInChildren<RadialMenu>())
            {
                Destroy(oldMenu.gameObject);
            }
            _Menu = Instantiate(RadialMenuPrefab, HUD.transform);
            _Menu.SetData(GameManager.CurrentLevelData.RecipePool);
            for (int i = 0; i < _Menu.Recipes.Count; i++)
            {
                Recipe recipe = _Menu.Recipes[i];
                if (!_CurrentInteractor.Inventory.HasItems(Query.MakeQuery(recipe.Input)))
                {
                    _Menu.Slots[i].Avaliable = false;
                }
            }
            _Menu.OnRecipeSelected += async (recipe) => { await OnRecipeSelected(recipe); };
            _Menu.OnPointerEnter += OnPointerEnterSlot;
            _Menu.OnPointerExit += OnPointerExitSlot;
            _Overlay = FindObjectOfType<UIOverlay>(true);
            _Overlay.gameObject.SetActive(true);
            _Overlay.Fade = 0.8f;
            _Overlay.FadeSpeed = 5f;
            _Overlay.OverlayTexture = Overlay;
            OnOpenRadialMenu?.Invoke();
        }

        private void HideRadialMenu()
        {
            if (_Menu != null)
            {
                Destroy(_Menu.gameObject);
            }
            SFXHelper.PlayOneshot(CloseSFX, GameObject);
            _Overlay = FindObjectOfType<UIOverlay>(true);
            _Overlay.gameObject.SetActive(false);
            _Overlay.Fade = 0f;
            OnCloseRadialMenu?.Invoke();
        }

        private async Task OnRecipeSelected(Recipe selectedRecipe)
        {
            if (!_CurrentInteractor.Inventory.TryWithdrawItems(out Dictionary<StorableObject, int> consumedItems, Query.MakeQuery(selectedRecipe.Input)))
            {
                return;
            }

            Inventory.DepositItems(consumedItems, ItemTypes.Ingedient, out consumedItems);

            _CurrentRecipe = selectedRecipe;

            Timer.StartNew(GetBaseWaitTime(selectedRecipe), true);

            HideRadialMenu();
            SFXHelper.PlayOneshot(SizzleSFX, gameObject);
            Animator.SetTrigger("Chomping");
            CookingStarted?.Invoke(_CurrentRecipe.Output[0]);
            await ProcessNewState(ProcessState.Active);
        }

        private void OnPointerEnterSlot(RadialMenuSlot slot)
        {
            if (!_SubMenus.ContainsKey(slot))
            {
                RadialMenu SubMenu = Instantiate(RadialSubMenuPrefab, slot.transform);
                SubMenu.SetData(slot.Recipe.Input.ToList());
                Dictionary<Food, int> ingredients = new Dictionary<Food, int>();
                for (int i = 0; i < SubMenu.Foods.Count; i++)
                {
                    Food food = SubMenu.Foods[i];
                    if (ingredients.ContainsKey(food))
                    {
                        if (!_CurrentInteractor.Inventory.HasItems(new Query(food, ingredients[food] + 1)))
                        {
                            SubMenu.Slots[i].Avaliable = false;
                        }
                        else
                        {
                            ingredients[food]++;
                        }
                    }
                    else
                    {
                        if (!_CurrentInteractor.Inventory.HasItems(new Query(food, 1)))
                        {
                            SubMenu.Slots[i].Avaliable = false;
                        }
                        else
                        {
                            ingredients.Add(food, 1);
                        }
                    }
                }
                _SubMenus.Add(slot, SubMenu);
            }
        }

        private void OnPointerExitSlot(RadialMenuSlot slot)
        {
            Destroy(_SubMenus[slot].gameObject);
            _SubMenus.Remove(slot);
        }

        public override void Start()
        {
            base.Start();
            _AmbientSFX = RuntimeManager.CreateInstance(AmbientSFX);
            _AmbientSFX.set3DAttributes(gameObject.To3DAttributes());
            _AmbientSFX.start();
            Timer.gameObject.SetActive(false);
        }

        public async Task ProcessNewState(ProcessState state)
        {
            State = state;
            switch (state)
            {
                case ProcessState.Ready:
                    if (GameManager.PlayerControls?.Player.Cancel.WasPressedThisFrame() == true)
                    {
                        HideRadialMenu();
                    }
                    break;

                case ProcessState.Active:
                    await Await.Seconds(GetCurrentWaitTime());

                    try
                    {
                        Inventory.RemoveItems(Query.MakeQuery(_CurrentRecipe.Input));
                        Inventory.DepositItems(Item.ToItems(_CurrentRecipe.Output), ItemTypes.OrderableFood);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(ex.Message);
                    }

                    Timer.gameObject.SetActive(false);

                    SFXHelper.PlayOneshot(ChimeSFX, gameObject);
                    SFXHelper.PlayOneshot(OpenSFX, GameObject);

                    await ProcessNewState(ProcessState.Complete);
                    CookingFinished?.Invoke(_CurrentRecipe.Output[0]);

                    Animator.SetTrigger("OvenOpen");
                    _OpenInteractTrigger.enabled = true;
                    await Await.Seconds(OvenOpenAnimation.length);

                    if (SpawnedFood)
                    {
                        Destroy(SpawnedFood);
                    }
                    SpawnedFood = Instantiate(_CurrentRecipe.Output[0].Prefab.gameObject, SpawnFoodLocation.transform.position, Quaternion.identity);
                    CookingFinished?.Invoke(_CurrentRecipe.Output[0]);
                    break;

                case ProcessState.Inactive:
                case ProcessState.Complete:
                    break;
            }
        }

        private float GetCurrentWaitTime() => GetWaitTime(_CurrentRecipe, BaseProcessTime);

        private float GetBaseWaitTime(Recipe recipe) => GetWaitTime(recipe, BaseProcessTime);

        private static float GetWaitTime(Recipe recipe, float time)
        {
            return recipe.ProcessTimeModifier switch
            {
                Modifier.Add => time + recipe.ProcessTime,
                Modifier.Multiply => time * recipe.ProcessTime,
                Modifier.Override => recipe.ProcessTime,
                _ => time
            };
        }

        private void OnDestroy()
        {
            _AmbientSFX.release();
        }
    }
}