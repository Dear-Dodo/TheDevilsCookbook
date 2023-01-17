using FMOD.Studio;
using FMODUnity;
using System;
using TDC.Core.Manager;
using TDC.Core.Utility;
using TDC.Interactions;
using TDC.Procedural;
using TDC.UI.HUD.InventoryWindow;
using Action = System.Action;

namespace TDC.Cooking
{
    public class TrashCan : Appliance
    {
        public EyeballController[] EyeControllers;

        public InventoryWidget InventoryWidget;

        public event Action OnInteract;

        public EventReference OpenSFX;
        public EventReference CloseSFX;
        public EventReference AmbientSFX;

        private EventInstance _AmbientSFX;

        public override Interaction GetInteractions(Interactor interactor)
        {
            Interaction interaction = Interaction.None;
            switch (State)
            {
                case Core.Utility.ProcessState.Inactive:
                    break;

                case Core.Utility.ProcessState.Ready:
                    interaction = Interaction.Activate;
                    break;

                case Core.Utility.ProcessState.Active:
                    break;

                case Core.Utility.ProcessState.Complete:
                    break;
            }
            return interaction;
        }

        public override void Interact(Interactor interactor, Interaction interaction)
        {
            if (interaction == Interaction.Activate)
            {
                SFXHelper.PlayOneshot(OpenSFX, GameObject);
                OnInteract?.Invoke();
                InventoryWidget.DeleteMode.Value = true;
                State = Core.Utility.ProcessState.Active;
                Animator.SetBool("Open", true);
            }
        }

        public override void OnHover(Interactor interactor)
        {
            base.OnHover(interactor);
            Array.ForEach(EyeControllers, a => { a.TargetObject = interactor.gameObject; });
        }

        public override void ExitHover(Interactor interactor)
        {
            SFXHelper.PlayOneshot(CloseSFX, GameObject);
            base.ExitHover(interactor);
            InventoryWidget.DeleteMode.Value = false;
            State = Core.Utility.ProcessState.Ready;
            Animator.SetBool("Open", false);
            Array.ForEach(EyeControllers, a => { a.TargetObject = a.DefaultTarget; });
        }

        public void Start()
        {
            base.Start();
            _AmbientSFX = RuntimeManager.CreateInstance(AmbientSFX);
            _AmbientSFX.set3DAttributes(gameObject.To3DAttributes());
            _AmbientSFX.start();    
            InventoryWidget ??= FindObjectOfType<InventoryWidget>();
            State = Core.Utility.ProcessState.Ready;
        }

        public void Update()
        {
            base.Update();
            if (GameManager.PlayerControls?.Player.Cancel.WasPressedThisFrame() == true)
            {
                InventoryWidget.DeleteMode.Value = false;
                State = Core.Utility.ProcessState.Ready;
                Animator.SetBool("Open", false);
            }
        }

        private void OnDestroy()
        {
            _AmbientSFX.release();
        }
    }
}