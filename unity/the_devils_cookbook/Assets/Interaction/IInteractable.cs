using UnityEngine;

namespace TDC.Interactions
{
    public enum Interaction
    {
        None,
        Deposit,
        Withdraw,
        Activate,
        Deactivate,
        Inspect
    }

    public interface IInteractable
    {
        public GameObject GameObject { get; }

        public Interaction GetInteractions(Interactor interactor);

        public void OnHover(Interactor interactor);

        public void ExitHover(Interactor interactor);

        public void Interact(Interactor interactor, Interaction interaction);
    }
}