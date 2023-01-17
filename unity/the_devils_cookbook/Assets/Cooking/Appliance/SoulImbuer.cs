using System;
using FMOD.Studio;
using FMODUnity;
using System.Collections.Generic;
using System.Linq;
using TDC.Core.Extension;
using TDC.Core.Manager;
using TDC.Core.Utility;
using TDC.Ingredient;
using TDC.Interactions;
using TDC.UI.Generic;
using TDC.UI.Menu;
using UnityAsync;
using UnityEngine;
using Utility;

namespace TDC.Cooking
{
    public class SoulImbuer : Appliance
    {
        public int MaxActiveIngredients;
        public Canvas HUD;
        
        public Texture2D Overlay;
        public AnimationClip SpitAnimation;
        public Transform SpawnPoint;
        public List<Creature> Ingredients;
        public RadialMenu RadialMenuPrefab;
        public RadialMenu RadialSubMenuPrefab;

        private int _ActiveIngredients;
        private RadialMenu _Menu;
        private Recipe _CurrentRecipe;
        public System.Action<List<Creature>> OnSpawnFood;
        public event Action<Recipe, List<Creature>> OnSpawnRecipe;
        public System.Action OnRadialMenuOpen;
        public System.Action OnRadialMenuClose;

        public EventReference SpitSFX;
        public EventReference ActiveSFX;
        public EventReference AmbientSFX;
        
        private EventInstance _AmbientSFX;

        private Dictionary<RadialMenuSlot, RadialMenu> SubMenus = new Dictionary<RadialMenuSlot, RadialMenu>();
        private UIOverlay _Overlay;
        private static readonly int _SpitAnimatorID = Animator.StringToHash("Spit");

        public override Interaction GetInteractions(Interactor interactor)
        {
            Interaction interaction = Interaction.None;
            switch (State)
            {
                case Core.Utility.ProcessState.Inactive:
                    interaction = Interaction.Inspect;
                    break;

                case Core.Utility.ProcessState.Ready:
                    interaction = Interaction.Activate;
                    break;

                case Core.Utility.ProcessState.Active:
                    interaction = Interaction.None;
                    break;

                case Core.Utility.ProcessState.Complete:
                    interaction = Interaction.None;
                    break;
            }
            return interaction;
        }

        public override void Interact(Interactor interactor, Interaction interaction)
        {
            switch (interaction)
            {
                case Interaction.None:
                    break;

                case Interaction.Activate:
                    SFXHelper.PlayOneshot(ActiveSFX, gameObject);
                    ShowRadialMenu();
                    break;

                case Interaction.Inspect:
                    break;
            }
        }

        public override void ExitHover(Interactor interactor)
        {
            base.ExitHover(interactor);
            HideRadialMenu();
        }

        private void ShowRadialMenu()
        {
            foreach (RadialMenu oldMenu in HUD.GetComponentsInChildren<RadialMenu>())
            {
                Destroy(oldMenu.gameObject);
            }
            _Menu = Instantiate(RadialMenuPrefab, HUD.transform);
            _Menu.SetData(GameManager.CurrentLevelData.RecipePool);
            SetMenuSlotAvailability(_ActiveIngredients < MaxActiveIngredients);
            _Menu.OnRecipeSelected += (recipe) => OnRecipeSelected(recipe);
            _Menu.OnPointerEnter += OnPointerEnterSlot;
            _Menu.OnPointerExit += OnPointerExitSlot;
            _Overlay = FindObjectOfType<UIOverlay>(true);
            _Overlay.gameObject.SetActive(true);
            _Overlay.Fade = 0.8f;
            _Overlay.FadeSpeed = 5f;
            _Overlay.OverlayTexture = Overlay;
            OnRadialMenuOpen?.Invoke();
        }

        private void HideRadialMenu()
        {
            if (_Menu != null)
            {
                Destroy(_Menu.gameObject);
            }
            _Overlay = FindObjectOfType<UIOverlay>(true);
            _Overlay.gameObject.SetActive(false);
            _Overlay.Fade = 0f;
            OnRadialMenuClose?.Invoke();
        }

        private void OnRecipeSelected(Recipe selectedRecipe)
        {
            SetMenuSlotAvailability(false);
            _CurrentRecipe = selectedRecipe;
            SpawnRecipe(selectedRecipe);
        }

        private void OnPointerEnterSlot(RadialMenuSlot slot)
        {
            if (!SubMenus.ContainsKey(slot))
            {
                RadialMenu SubMenu = Instantiate(RadialSubMenuPrefab, slot.transform);
                SubMenu.SetData(slot.Recipe.Input.ToList());
                SubMenus.Add(slot, SubMenu);
            }
        }

        private void OnPointerExitSlot(RadialMenuSlot slot)
        {
            Destroy(SubMenus[slot].gameObject);
            SubMenus.Remove(slot);
        }

        public Creature SpawnCreature(Creature toSpawn, Vector3 position)
        {
            Creature instance = CreatureManager.CreateCreature(toSpawn, position);
            instance.Launch(SpawnPoint.position, position);
            instance.OnCaught += () => _ActiveIngredients--;
            _ActiveIngredients++;
            return instance;
        }

        private void SetMenuSlotAvailability(bool isAvailable)
        {
            foreach (RadialMenuSlot slot in _Menu.Slots)
            {
                slot.Avaliable = isAvailable;
            }
        }
        
        private async void SpawnRecipe(Recipe recipe)
        {
            await Await.Seconds(BaseProcessTime);
            Animator.SetTrigger(_SpitAnimatorID);
            await Await.Seconds(SpitAnimation.length * 0.5f);
            SFXHelper.PlayOneshot(SpitSFX, gameObject);

            Vector2 spawnCentre = GameManager.CurrentLevelData.PoissonDisc.Points.Random(GameManager.GameRandom);
            List<Vector2> points = GameManager.CurrentLevelData.PoissonDisc.GetPointsInRadius(spawnCentre, 5.0f);

            var spawned = new List<Creature>(recipe.Input.Length);
            foreach (Food toSpawn in recipe.Input)
            {
                Vector3 spawnPoint = points.Random(GameManager.GameRandom).xoy();
                if (toSpawn.Creature == null)
                    throw new ArgumentException(
                        $"Attempted to spawn food with no associated creature. Food: {toSpawn.Name} in recipe {recipe.name}");
                spawned.Add(SpawnCreature(toSpawn.Creature, spawnPoint));
            }
            OnSpawnFood?.Invoke(spawned);
            OnSpawnRecipe?.Invoke(recipe, spawned);
            SetMenuSlotAvailability(_ActiveIngredients < MaxActiveIngredients);
        }

        public override void Start()
        {
            base.Start();
            _AmbientSFX = RuntimeManager.CreateInstance(AmbientSFX);
            _AmbientSFX.set3DAttributes(gameObject.To3DAttributes());
            _AmbientSFX.start();
        }

        private void OnDestroy()
        {
            _AmbientSFX.release();
        }
    }
}