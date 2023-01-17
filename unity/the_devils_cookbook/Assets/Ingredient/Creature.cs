using FMODUnity;
using System.Collections.Generic;
using TDC.AIRefac;
using TDC.Core.Manager;
using TDC.Core.Utility;
using TDC.Items;
using UnityEngine;

namespace TDC.Ingredient
{
    public class Creature : MonoBehaviour
    {
        public float SpawnLaunchSpeed;
        public float SpawnLaunchHeight;
        [SerializeField, SerializedValueRequired]
        private Food _Food;

        public Food ContainedFood => _Food;

        public EventReference CaughtSFX;

        private TrailRenderer _TrailRenderer;
        private Agent _Agent;
        public event System.Action OnCaught;
        private float _SpawnLerp;
        private bool _Launching = false;
        private Vector3 _CurveStart;
        private Vector3 _CurveEnd;
        private Vector3 _ControlPoint;
        private List<Vector3> _Curve;
        private bool _NearPlayer = false;
        private static readonly int _IsFinishedSpawning = Animator.StringToHash("IsFinishedSpawning");

        public Item Catch()
        {
            SFXHelper.PlayOneshot(CaughtSFX, gameObject);
            OnCaught?.Invoke();
            Item item = _Food.CreateItem();
            item.transform.position = transform.position;
            CreatureManager.DestroyCreature(this);
            return item;
        }

        public Food PeekData()
        {
            return _Food;
        }
        
        public void Launch(Vector3 startPoint, Vector3 endPoint)
        {
            _CurveStart = startPoint;
            _CurveEnd = endPoint;
            GenerateCurve();
            if (_Agent != null) _Agent.enabled = false;
            var refacAgent = GetComponent<AIRefac.Agent>();
            if (refacAgent != null) refacAgent.enabled = false;
            _TrailRenderer.enabled = true;
            _SpawnLerp = 0;
            _Launching = true;
        }

        public void Activate()
        {
            if (_Agent != null) _Agent.enabled = true;
            var refacAgent = GetComponent<AIRefac.Agent>();
            if (refacAgent != null)
            {
                refacAgent.enabled = true;
                refacAgent.Enable();
            }
            _TrailRenderer.enabled = false;
            _Launching = false;
            GetComponentInChildren<Animator>()?.SetBool(_IsFinishedSpawning, true);
        }

        private void GenerateCurve()
        {
            _ControlPoint = Vector3.Lerp(_CurveStart, _CurveEnd, 0.5f) + new Vector3(0, SpawnLaunchHeight, 0);

            _Curve = new List<Vector3>
            {
                _CurveStart,
                _ControlPoint,
                _CurveEnd
            };
        }



        private void Awake()
        {
            SerializedFieldValidation.Validate(GetType(), this, true);
            SerializedFieldValidation.Validate(typeof(Food), _Food, true);
            _Agent = GetComponent<Agent>();
            _TrailRenderer = GetComponentInChildren<TrailRenderer>();
        }


        private void Update()
        {
            if (_Launching) {
                transform.position = Bezier.CalculatePoint(_Curve, _SpawnLerp);
                _SpawnLerp += SpawnLaunchSpeed * Time.deltaTime;
                if (_SpawnLerp >= 1)
                {
                    transform.position = _CurveEnd;
                    Activate();
                }
            }
        }
    }
}