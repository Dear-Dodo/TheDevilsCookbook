using System;
using TDC.Core.Manager;
using TDC.Core.Utility;
using TDC.Interactions;
using TDC.Items;
using TDC.ThirdParty.SerializableDictionary;
using TDC.UI;
using UnityEngine;

namespace TDC.Cooking
{
    [RequireComponent(typeof(Inventory))]
    public abstract class Appliance : MonoBehaviour, IInteractable
    {
        public GameObject GameObject => gameObject;

        public RecipeList RecipeList;
        public ProcessState State;

        public ButtonPrompt ButtonPrompt;

        public float BaseProcessTime;

        public Animator Animator;

        protected float ElapsedProcessTime;
        protected bool ShowPrompt = false;
        protected bool Hovering = false;

        public Inventory Inventory;
        public SerializableDictionary<Interaction, string> ButtonPrompts;

        public event Action OnHoverEnter;

        public event Action OnHoverExit;

        public virtual void Awake()
        {
            ButtonPrompt ??= GetComponentInChildren<ButtonPrompt>();
            GameManager.RunOnInitialisation(() => ButtonPrompt.SetButton(GameManager.PlayerControls.Player.Interact));
            ButtonPrompt.SetActive(false);

            Inventory = GetComponent<Inventory>();
        }

        public virtual async void Start()
        {
            await GameManager.LevelInitialisedAsync.WaitAsync();
            ButtonPrompt.ClickButton.onClick.AddListener(GameManager.PlayerCharacter.Interact);
        }

        public virtual void OnHover(Interactor interactor)
        {
            if (!Hovering)
            {
                OnHoverEnter?.Invoke();
                Hovering = true;
            }
            Interaction interaction = GetInteractions(interactor);
            if (interaction != Interaction.None)
            {
                ShowPrompt = true;
                ButtonPrompt.SetPrompt(ButtonPrompts[interaction]);
            }
            else
            {
                ShowPrompt = false;
            }
        }

        public virtual void Update()
        {
            if (ShowPrompt && !GameManager.PlayerCharacter.Spellcaster.IsSelectingForCast)
            {
                ButtonPrompt.SetActive(true);
                ShowPrompt = false;
            }
            else
            {
                ButtonPrompt.SetActive(false);
            }
        }

        public abstract Interaction GetInteractions(Interactor interactor);

        public abstract void Interact(Interactor interactor, Interaction interaction);

        public virtual void ExitHover(Interactor interactor)
        {
            OnHoverExit?.Invoke();
            Hovering = false;
        }
    }
}