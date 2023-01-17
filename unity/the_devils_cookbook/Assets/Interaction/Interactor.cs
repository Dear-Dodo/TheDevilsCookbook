using System.Collections.Generic;
using TDC.Affectables;
using UnityEngine;
using TDC.Items;

namespace TDC.Interactions
{
    [RequireComponent(typeof(Inventory))]
    public class Interactor : MonoBehaviour
    {
        [SerializeField]
        private float _Range;

        public Inventory Inventory;
        private List<IInteractable> _Interactables = new List<IInteractable>();
        private List<IInteractable> _PreviousInteractables = new List<IInteractable>();
        private IInteractable closestInteractable;

        private AffectableStats _Stats;

        public bool Interact(Interaction interaction)
        {
            if (_Stats.ModifiedStats["Stunned"] > 0) return false;
            IInteractable target = GetMostRelevantInteractable();
            if (target != null)
            {
                target.Interact(this, interaction);
                return true;
            }
            return false;
        }

        public List<IInteractable> TryGetInteractables()
        {
            closestInteractable = null;
            Collider[] nearbyColliders = Physics.OverlapSphere(transform.position, _Range);

            _Interactables = new List<IInteractable>();
            foreach(Collider nearbyObject in nearbyColliders)
            {
                IInteractable nearbyInteractable = nearbyObject.gameObject.GetComponent<IInteractable>();
                if (nearbyInteractable != null)
                {
                    _Interactables.Add(nearbyInteractable);
                    nearbyInteractable.OnHover(this);
                }
            }
            foreach (IInteractable interactable in _PreviousInteractables)
            {
                if (!_Interactables.Contains(interactable))
                {
                    interactable.ExitHover(this);
                }
            }
            _PreviousInteractables = new List<IInteractable>();
            _PreviousInteractables.AddRange(_Interactables);
            return _Interactables;
        }

        public IInteractable GetMostRelevantInteractable()
        {
            if (closestInteractable == null)
            {
                float sqrdist = float.PositiveInfinity;
                foreach (IInteractable interactable in _Interactables)
                {
                    Vector3 dist = interactable.GameObject.transform.position - transform.position;
                    if (dist.sqrMagnitude <= sqrdist)
                    {
                        sqrdist = dist.sqrMagnitude;
                        closestInteractable = interactable;
                    }
                }
            }
            return closestInteractable;
        }

        public Interaction GetInteractions()
        {
            IInteractable closestInteractable = GetMostRelevantInteractable();
            if (closestInteractable != null) {
                return GetMostRelevantInteractable().GetInteractions(this);
            }
            return Interaction.None;
        }

        public void Start()
        {
            _Stats = GetComponent<AffectableStats>();
            Inventory = GetComponent<Inventory>();
        }

        private void Update()
        {
            TryGetInteractables();
        }
    }
}
