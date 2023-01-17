using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;
using Utility;

namespace TDC.Spellcasting.Selectors
{
    public class LineSelector : Selector
    {
        public class LineSelectorSettings : SelectorSettings
        {
            /// <summary>
            /// Maximum distance the line can travel total. '0' to disable.
            /// </summary>
            public float MaxLineDistance = 0;

            /// <summary>
            /// Maximum distance between two connected points. '0' to disable.
            /// </summary>
            public float MaxSingleLineDistance = 0;

            /// <summary>
            /// Maximum distance from source. '0' to disable.
            /// </summary>
            public float MaxCastRange = 0;

            /// <summary>
            /// Create a valid target as far as possible towards the cursor when the distance is exceeded.
            /// </summary>
            public bool ClampWhenTooFar = true;

            /// <summary>
            /// Are lines that pass through obstacles considered invalid.
            /// </summary>
            public bool IsBlockedByObstacles = true;
        }

        public struct VFXDelegate
        {
            public bool IsAppliedToSelf;
            public Action<VisualEffect> Delegate;
        }

        public struct ValidityChangeData
        {
            public List<VisualEffect> SecondaryVFX;
            public VisualEffect SelectorVFX;
            public bool IsValid;
            public bool IsBlocked;
        }

        private float _CurrentLength = 0;
        private VisualEffectAsset _VisualEffectAsset;
        private List<VisualEffect> _VFXObjects;

        private readonly List<VFXDelegate> _VFXActions = new List<VFXDelegate>();

        public event Action<ValidityChangeData> ValidityChanged;

        private bool _LastValidityState = false;
        private float _LastLineLength = 0;

        protected override SelectorSettings _Settings { get; set; } = new LineSelectorSettings();

        public new LineSelectorSettings Settings
        {
            get => (LineSelectorSettings)_Settings;
            set => _Settings = value;
        }

        public void AddVFXDelegate(Action<VisualEffect> onCreate, bool isAppliedToSelf)
        {
            _VFXActions.Add(new VFXDelegate() { Delegate = onCreate, IsAppliedToSelf = isAppliedToSelf });
        }

        protected override void OnSelectionStart()
        {
            _VFXObjects = new List<VisualEffect>(Settings.TargetCount);
            foreach (VFXDelegate vfxDelegate in _VFXActions.Where(vfxDelegate => vfxDelegate.IsAppliedToSelf))
            {
                vfxDelegate.Delegate(VisualEffect);
            }
        }

        protected override void OnSelectionEnd()
        {
            foreach (VisualEffect vfx in _VFXObjects)
            {
                vfx.Stop();
                Destroy(vfx.gameObject, Settings.ShouldDestroyImmediately ? 0 : 5.0f);
            }
            _VFXObjects.Clear();
            _VFXActions.Clear();
            ValidityChanged = null;
        }

        protected void CreateNewVFX(Vector3 anchor)
        {
            var vfxObject = new GameObject($"VFX_LineSelector_{_VFXObjects.Count:00}")
            {
                transform =
                {
                    position = anchor
                }
            };
            var vfx = vfxObject.AddComponent<VisualEffect>();
            vfx.visualEffectAsset = _VisualEffectAsset;
            _VFXObjects.Add(vfx);
            vfx.SetVector3("Start", anchor);
            vfx.SetVector3("End", LastSelectedTarget?.Position ?? anchor);

            foreach (VFXDelegate vfxDelegate in _VFXActions)
            {
                vfxDelegate.Delegate(vfx);
            }
        }

        protected override void SetVFXGraphics(Spell.SpellGraphics graphics)
        {
            _VisualEffectAsset = graphics.VFX;
            VisualEffect.visualEffectAsset = graphics.VFX;
        }

        protected void SetLastVFXEnd(Vector3 pos)
        {
            if (_VFXObjects.Count == 0) return;
            VisualEffect vfx = _VFXObjects[_VFXObjects.Count - 1];
            Transform vfxTransform = vfx.transform;
            vfx.SetVector3("End", pos);
            Vector3 start = vfx.GetVector3("Start");
            Vector3 line = pos - start;
            vfxTransform.position = (start + pos) / 2;
            if (line.sqrMagnitude > 0) vfxTransform.forward = line;
            vfxTransform.localScale = new Vector3(1, 1, line.magnitude);
        }

        private void ClampLinePosition(Vector3 previous, ref Vector3 current, float lineLength, float newTotalLength)
        {
            float exceededSingle = Settings.MaxSingleLineDistance == 0
                ? 0
                : lineLength - Settings.MaxSingleLineDistance;
            float exceededTotal = Settings.MaxLineDistance == 0
                ? 0
                : newTotalLength - Settings.MaxLineDistance;
            float distanceExceeded = Mathf.Max(exceededSingle, exceededTotal);
            current = Vector3.MoveTowards(current, previous, distanceExceeded);
        }

        private bool IsValidTarget(ref Vector3 pos, out float lineLength)
        {
            float castDistance = Vector2.Distance(pos.xz(), Source.transform.position.xz());
            bool isInRange = Settings.MaxCastRange == 0 || castDistance <= Settings.MaxCastRange;

            lineLength = Targets.Count == 0 ? 0 :
                Vector2.Distance(Targets[Targets.Count - 1].Position.xz(), pos.xz());
            bool isLineDistanceValid = Settings.MaxSingleLineDistance == 0
                                       || lineLength <= Settings.MaxSingleLineDistance;

            float newTotalLength = _CurrentLength + lineLength;
            bool isTotalLengthValid = Settings.MaxLineDistance == 0 ||
                                      newTotalLength <= Settings.MaxLineDistance;

            Vector3 previous = Targets.Any() ? Targets.Last().Position : Vector3.zero;
            if ((!isLineDistanceValid || !isTotalLengthValid) && Settings.ClampWhenTooFar)
            {
                ClampLinePosition(previous, ref pos, lineLength, newTotalLength);
                isLineDistanceValid = true;
                isTotalLengthValid = true;
            }

            bool isValid = isInRange && isLineDistanceValid && isTotalLengthValid;

            var isBlocked = false;

            if (isValid && Settings.IsBlockedByObstacles && Targets.Any())
            {
                isBlocked = Physics.Linecast(previous, pos, out RaycastHit hit, ~LayerMask.GetMask("Castable Surface"), QueryTriggerInteraction.Ignore);
                isValid = !isBlocked;
            }

            switch (isValid)
            {
                case true when _LastValidityState == false:
                    ValidityChanged?.Invoke(CreateValidityData(true, false));
                    _LastValidityState = true;
                    break;

                case false when _LastValidityState:
                    ValidityChanged?.Invoke(CreateValidityData(false, isBlocked));
                    _LastValidityState = false;
                    break;
            }

            return isValid;
        }

        protected override void RequestTargetAdd(Target toAdd)
        {
            if (!_LastValidityState) return;
            _CurrentLength += _LastLineLength;
            Targets.Add(toAdd);
            SetLastVFXEnd(toAdd.Position);
            CreateNewVFX(toAdd.Position);
        }

        private ValidityChangeData CreateValidityData(bool isValid, bool isBlocked)
        {
            return new ValidityChangeData()
            {
                IsValid = isValid,
                IsBlocked = isBlocked,
                SecondaryVFX = _VFXObjects,
                SelectorVFX = VisualEffect
            };
        }

        protected override void UpdateSelection()
        {
            Ray ray = MainCamera.ScreenPointToRay(new Vector3(Mouse.current.position.x.ReadValue(),
                Mouse.current.position.y.ReadValue(), 0));
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, CastableSurfaceBitMask))
            {
                Vector3 point = hit.point;
                Debug.DrawLine(point, point + Vector3.up,Color.yellow);
                IsValidTarget(ref point, out float lineLength);
                LastSelectedTarget = new Target(point);
                transform.position = point;
                VisualEffect.SetVector3("Start", point);
                _LastLineLength = lineLength;
                SetLastVFXEnd(point);
                return;
            }

            if (_LastValidityState)
            {
                ValidityChanged?.Invoke(CreateValidityData(false, false));
                _LastValidityState = false;
            }
            LastSelectedTarget = null;
        }
    }
}