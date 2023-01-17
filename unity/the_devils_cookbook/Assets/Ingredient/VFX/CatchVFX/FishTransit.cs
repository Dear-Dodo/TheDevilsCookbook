using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using TDC.Core.Utility;
using UnityEngine;
using UnityEngine.VFX;

namespace TDC.VFX
{
    public class IngredientTransit : MonoBehaviour
    {
        public Action TransitFinished;
        
        private const float _TransitDuration = 3.0f;
        private Vector3 _StartPosition;
        private Vector3 _TargetPosition;
        private Vector3 _MidBezierPoint;
        
        public AnimationCurve SpinRadiusCurve;
        public float BezierStrength = 5.0f;

        public float ReformDuration = 4.0f;
        
        private float _CurrentTime = 0.0f;


        [SerializeField] VisualEffect _ExplosionEffect;
        [SerializeField] private VisualEffect _TransitEffect;
        [SerializeField] private VisualEffect _ReformEffect;

        private void Awake()
        {
            _ExplosionEffect.enabled = true;
            _ExplosionEffect.Stop();
            _TransitEffect.enabled = true;
            _TransitEffect.Stop();
            _ReformEffect.enabled = true;
            _ReformEffect.Stop();
        }
        
        public void SetColour(Color colour)
        {
            _TransitEffect.SetVector4("Colour", new Vector4(colour.r,colour.g,colour.b, colour.a));
            _ExplosionEffect.SetVector4("Colour", new Vector4(colour.r,colour.g,colour.b, colour.a));
            _ReformEffect.SetVector4("Colour", new Vector4(colour.r,colour.g,colour.b, colour.a));
        }

        public void SetSpinRadius(float radius)
        {
            _TransitEffect.SetFloat("SpinRadius", Mathf.Max(0.01f,radius));
        }

        public void SetReformSpawnRadius(float radius)
        {
            _ReformEffect.SetFloat("SpawnRadius", Mathf.Max(0.01f, radius));
        }

        public Task StartTransit(Vector3 target, Color colour)
        {
            _TargetPosition = target;
            _StartPosition = transform.position;
            _MidBezierPoint = (_StartPosition + _TargetPosition) / 2.0f + 
                              Vector3.Cross(_TargetPosition - _StartPosition,Vector3.up).normalized * 5.0f;
            
            SetSpinRadius(SpinRadiusCurve.Evaluate(0));
            SetColour(colour);

            _CurrentTime = 0.0f;
            return DoTransit();
        }

        private async Task DoTransit()
        {
            _TransitEffect.enabled = true;
            _TransitEffect.Play();
            _ExplosionEffect.Play();
            await DOTween.To(() => _CurrentTime, ProgressTransit, 1, _TransitDuration).AsyncWaitForCompletion();
            _CurrentTime = 0.0f;
            _ReformEffect.Play();
            var startRadius = SpinRadiusCurve.Evaluate(1);
            await DOTween.To(() => _CurrentTime, (t) => ProgressReform(t, startRadius), 1, ReformDuration).AsyncWaitForCompletion();
            _TransitEffect.Stop();
            _ReformEffect.Stop();

            _TransitEffect.Stop();
            _TransitEffect.enabled = false;

            TransitFinished?.Invoke();
        }

        private void ProgressTransit(float t)
        {
            _CurrentTime = t;
            var bezierPos =
                Bezier.CalculatePoint(new List<Vector3>() { _StartPosition, _MidBezierPoint, _TargetPosition },
                    _CurrentTime);
            SetSpinRadius(SpinRadiusCurve.Evaluate(t));
            var displacement = bezierPos - transform.position;
            if (t < 1)
            {
                transform.forward = displacement;
            }
            transform.position = bezierPos;
        }

        private void ProgressReform(float t, float startRadius)
        {
            _CurrentTime = t;
            var radii = Mathf.Lerp(startRadius, 0.01f, t);
            SetReformSpawnRadius(radii * 1.5f);
            SetSpinRadius(radii);
        }
    }
}
