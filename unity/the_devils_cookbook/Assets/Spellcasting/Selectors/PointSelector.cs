using System;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;
using Utility;

namespace TDC.Spellcasting.Selectors
{
    public class PointSelector : Selector
    {
        public class PointSelectorSettings : SelectorSettings
        {
            /// <summary>
            /// Maximum distance between any two target points. '0' to disable.
            /// </summary>
            public float MaxDistanceBetweenPoints = 0;

            /// <summary>
            /// Maximum distance from source. '0' to disable.
            /// </summary>
            public float MaxCastRange = 0;
        }

        public struct ValidityChangeData
        {
            public PointSelector Selector;
            public VisualEffect SelectorVFX;
            public bool IsValid;
        }

        public event Action<ValidityChangeData, PointSelector> OnValidityChanged;

        protected override SelectorSettings _Settings { get; set; } = new PointSelectorSettings();

        public new PointSelectorSettings Settings
        {
            get => (PointSelectorSettings)_Settings;
            set => _Settings = value;
        }

        private bool _LastValidityState = true;

        protected override void RequestTargetAdd(Target toAdd)
        {
            float castDistance = Vector2.Distance(toAdd.Position.xz(), Source.transform.position.xz());
            bool isInRange = Settings.MaxCastRange == 0 || castDistance <= Settings.MaxCastRange;

            bool areTargetsInProximity = Settings.MaxDistanceBetweenPoints == 0 ||
                                         Targets.All(t => Vector2.Distance(
                                             t.AffectableUnit.transform.position.xz(), LastSelectedTarget.Position.xz())
                                                          <= Settings.MaxDistanceBetweenPoints);

            if (isInRange && areTargetsInProximity) Targets.Add(toAdd);
        }

        protected override void UpdateSelection()
        {
            Ray ray = MainCamera.ScreenPointToRay(new Vector3(Mouse.current.position.x.ReadValue(),
                Mouse.current.position.y.ReadValue(), 0));
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, CastableSurfaceBitMask))
            {
                IsValidTarget(hit.point);
                LastSelectedTarget = new Target(hit.point);
                transform.position = hit.point;
                return;
            }

            if (_LastValidityState)
            {
                OnValidityChanged?.Invoke(CreateValidityData(false), this);
                _LastValidityState = false;
            }

            LastSelectedTarget = null;
        }

        private bool IsValidTarget(Vector3 pos)
        {
            float castDistance = Vector3.Distance(pos.xz(), Source.transform.position.xz());
            bool isValid = Settings.MaxCastRange == 0 || castDistance <= Settings.MaxCastRange;

            switch (isValid)
            {
                case true when _LastValidityState == false:
                    OnValidityChanged?.Invoke(CreateValidityData(true), this);
                    _LastValidityState = true;
                    break;

                case false when _LastValidityState:
                    OnValidityChanged?.Invoke(CreateValidityData(false), this);
                    _LastValidityState = false;
                    break;
            }

            return isValid;
        }

        private ValidityChangeData CreateValidityData(bool isValid) =>
            new ValidityChangeData()
            {
                IsValid = isValid,
                SelectorVFX = VisualEffect,
                Selector = this
            };
    }
}