using FMOD.Studio;
using FMODUnity;
using System;
using System.Linq;
using DG.Tweening;
using TDC.Affectables;
using TDC.Core.Manager;
using TDC.Core.Utility;
using UnityAsync;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using Utility;

namespace TDC.Player
{
    [RequireComponent(typeof(AffectableStats))]
    public class PlayerMovement : MonoBehaviour
    {
        [Header("Dash")]
        [Min(0.1f)]
        public float DashSpeedFactor;
        [Min(0.1f)]
        public float DashDistance;
        public float DashDecelerationTime;
        public float DashCooldown;
        [Min(1)]
        public int MaxDashCharges;

        [SerializeField, SerializedValueRequired]
        private GameObject _DashChargeOrbPrefab;
        [SerializeField] private float _ChargeOrbScale = 0.25f;
        [SerializeField] private float _ChargeOrbHorizontalOffset = 0.75f;
        [SerializeField] private float _ChargeOrbVerticalOffset = 2.0f;
        [SerializeField] private float _ChargeOrbRotationSpeed = 36;
        [SerializeField] private float _ChargeOrbFadeTime = 0.1f;
        [SerializeField, SerializedValueRequired]
        private Material _DashMaterial;
        
        private (Renderer renderer, Material[] materials)[] _RendererValues;


        [Header("Movement")]
        public float Acceleration;
        public float Deceleration;
        public float RotationSpeed;

        private PlayerCharacter _PlayerCharacter;
        private Rigidbody _Rigidbody;
        private Collider _Collider;
        public Animator AnimatoionController;
        private GameObject _PlayerModel;
        private AffectableStats _Stats;
            
        private bool _IsDashing;
        private float _DashTime;
        private float _CurrentDashCooldown;
        private int _CurrentDashCharges;

        private Transform[] _DashChargeOrbs;

        private static readonly int _MoveSpeed = Animator.StringToHash("MoveSpeed");

        private void CreateDashChargeOrbs()
        {
            float degreesPerCharge = 365f / MaxDashCharges;
            Vector3 origin = transform.position;
            var offset = new Vector3(0, _ChargeOrbVerticalOffset, _ChargeOrbHorizontalOffset);
            _DashChargeOrbs = new Transform[MaxDashCharges];
            for (var i = 0; i < MaxDashCharges; i++)
            {
                Vector3 position = origin + Quaternion.AngleAxis(degreesPerCharge * i, Vector3.up) * offset;
                _DashChargeOrbs[i] = Instantiate(_DashChargeOrbPrefab, transform).transform;
                _DashChargeOrbs[i].position = position;
                _DashChargeOrbs[i].localScale = new Vector3(_ChargeOrbScale, _ChargeOrbScale, _ChargeOrbScale);
            }
        }

        private void ConsumeDashCharge()
        {
            _CurrentDashCharges--;
            _DashChargeOrbs[_CurrentDashCharges].DOScale(0, _ChargeOrbFadeTime);
        }

        private void AddDashCharge()
        {
            _DashChargeOrbs[_CurrentDashCharges].DOScale(_ChargeOrbScale, _ChargeOrbFadeTime);
            _CurrentDashCharges++;
        }
        
        private async void Awake()
        {
            this.Validate();
            _PlayerCharacter = GetComponent<PlayerCharacter>();
            _Stats = GetComponent<AffectableStats>();
            _Rigidbody = GetComponent<Rigidbody>();
            _Collider = GetComponent<Collider>();
            AnimatoionController = GetComponentInChildren<Animator>();

            _RendererValues = GetComponentsInChildren<Renderer>()
                .Select<Renderer, (Renderer, Material[])>(r => (r, r.materials)).ToArray();

            _PlayerModel = AnimatoionController.gameObject;
            GameManager.RunOnInitialisation(() => GameManager.PlayerControls.Player.Run.performed += Dash);
            _CurrentDashCharges = MaxDashCharges;
            CreateDashChargeOrbs();

            (await _PlayerCharacter.GetPlayerStats()).Health.OnValueChanged += FlashWhenHit;
            // if (DashDecelerationTime > MoveSpeed) DashDecelerationTime = MoveSpeed;
        }

        private void OnDestroy()
        {
            GameManager.PlayerControls.Player.Run.performed -= Dash;
        }

        private Vector3 GetDashDirectionFromCursor()
        {
            Camera main = Camera.main;
            if (main == null) throw new NullReferenceException("No main camera for cursor dash.");
            Ray ray = main.ScreenPointToRay(new Vector3(Mouse.current.position.x.ReadValue(),
                Mouse.current.position.y.ReadValue(), 0));
            if (!new Plane(Vector3.up, transform.position).Raycast(ray, out float dist))
            {
                throw new Exception($"Raycast did not hit floor plane for cursor dash.");
            }

            Vector3 point = ray.GetPoint(dist);
            return (point - transform.position).normalized;
        }
        
        private Vector3 GetDashDirectionFromInput()
        {
            var input = GameManager.PlayerControls.Player.Move.ReadValue<Vector2>();
            if (input.magnitude > 0.01f)
            {
                return input.xoy().normalized;
            } else
            {
                return _PlayerModel.transform.rotation * Vector3.forward;
            }
        }

        private async void Dash(InputAction.CallbackContext _)
        {
            bool isStunned = _Stats.ModifiedStats["Stunned"] > 0;
            bool canDash = _CurrentDashCharges > 0;
            if (_IsDashing || !canDash || isStunned) return;
            
            bool dashUsingInput = GameManager.UserSettings.DashMode == UserSettings.DashDirectionMode.TowardsMovement;
            Vector3 direction = dashUsingInput ? GetDashDirectionFromInput() : GetDashDirectionFromCursor();
            Transform playerTransform = transform;


            const float halfCapsuleHeight = 0.7f;
            const float capsuleRadius = 0.5f;
            
            var capsuleOffset = new Vector3(0, halfCapsuleHeight, 0);
            const float dashCollisionCheckInterval = 0.1f;
            const float maxSampleDistance = 0.1f;

            float actualDistance = DashDistance * _Stats.ModifiedStats["Movement Speed"] / _Stats.BaseStats["Movement Speed"];
            Vector3 dashEndPosition = default;
            float currentCheckOffset = actualDistance;
            while (currentCheckOffset > 0)
            {
                Vector3 currentOrigin = playerTransform.position + direction * currentCheckOffset;
                if (NavMesh.SamplePosition(currentOrigin, out NavMeshHit hit, maxSampleDistance, NavMesh.AllAreas))
                {
                    dashEndPosition = hit.position.xoz() + playerTransform.position.oyo();
                    actualDistance = currentCheckOffset;
                    break;
                }
                currentCheckOffset -= dashCollisionCheckInterval;
            }
            if (currentCheckOffset <= 0) return;

            if (_CurrentDashCharges == MaxDashCharges) _CurrentDashCooldown = DashCooldown;
            ConsumeDashCharge();
            _IsDashing = true;
            
            foreach ((Renderer subRenderer, Material[] materials) in _RendererValues)
            {
                subRenderer.material = _DashMaterial;
            }

            _Collider.isTrigger = true;

            float dashSpeed = _Stats.ModifiedStats["Movement Speed"] * DashSpeedFactor;
            float duration = actualDistance / dashSpeed;
            
            EventInstance dashAudio = RuntimeManager.CreateInstance(_PlayerCharacter.DashAudioEvent);
            dashAudio.set3DAttributes(RuntimeUtils.To3DAttributes(gameObject));
            dashAudio.start();
            dashAudio.release();

            _PlayerModel.transform.rotation = Quaternion.LookRotation(direction);
            Vector3 dashStartPosition = playerTransform.position;
            
            float elapsed = 0;
            while (elapsed < duration)
            {
                playerTransform.position = Vector3.Lerp(dashStartPosition, dashEndPosition, elapsed / duration);
                elapsed += Time.fixedDeltaTime;
                await new WaitForFixedUpdate();
                await Await.NextFixedUpdate().ConfigureAwait(FrameScheduler.FixedUpdate);
            }
            foreach ((Renderer subRenderer, Material[] materials) in _RendererValues)
            {
                subRenderer.materials = materials;
            }

            _Collider.isTrigger = false;
            _IsDashing = false;
        }

        private void UpdateChargeOrbPositions()
        {
            float deltaRotation = _ChargeOrbRotationSpeed * Time.fixedDeltaTime;
            Vector3 rotationOrigin = transform.position;
            foreach (Transform orb in _DashChargeOrbs)
            {
                orb.RotateAround(rotationOrigin, Vector3.up, deltaRotation);
            }
        }
        private void UpdateDashCooldown()
        {
            if (_CurrentDashCharges >= MaxDashCharges) return;

            _CurrentDashCooldown -= Time.fixedDeltaTime;
            if (_CurrentDashCooldown > 0) return;
            AddDashCharge();
            _CurrentDashCooldown = DashCooldown;
        }
        
        private async void FlashWhenHit(int oldHealth, int newHealth)
        {
            if (newHealth < oldHealth)
            {
                foreach ((Renderer subRenderer, Material[] materials) in _RendererValues)
                {
                    subRenderer.material = _DashMaterial;
                }
                await Await.Seconds(1f);
                foreach ((Renderer subRenderer, Material[] materials) in _RendererValues)
                {
                    subRenderer.materials = materials;
                }
            }
        }

        private void FixedUpdate()
        {
            UpdateDashCooldown();
            UpdateChargeOrbPositions();
            if (!_IsDashing)
            {
                
                Vector2 input = GameManager.PlayerControls?.Player.Move.ReadValue<Vector2>() ?? Vector2.zero;
                if (_Stats.ModifiedStats["Stunned"] > 0) input = Vector2.zero;
                float targetSpeed = _Stats.ModifiedStats["Movement Speed"];
                targetSpeed *= input.magnitude;

                var delta = new Vector3(input.x, 0, input.y);
                float velocity = Vector3.Dot(_Rigidbody.velocity, delta.normalized);
                float deltaV = velocity < targetSpeed ? Acceleration : Deceleration;
                Vector3 targetVelocity = delta.normalized * targetSpeed;
                Vector3 targetDirection = targetVelocity - _Rigidbody.velocity;
                delta = Mathf.Min(deltaV * Time.fixedDeltaTime, targetDirection.magnitude) * targetDirection.normalized;
                _Rigidbody.velocity += delta;

            }
            if (_Rigidbody.velocity != Vector3.zero)
            {
                _PlayerModel.transform.rotation =
                    Quaternion.Slerp(_PlayerModel.transform.rotation, Quaternion.LookRotation(_Rigidbody.velocity), RotationSpeed);
            }
            AnimatoionController.SetFloat(_MoveSpeed, _Rigidbody.velocity.magnitude);
        }
    }
}