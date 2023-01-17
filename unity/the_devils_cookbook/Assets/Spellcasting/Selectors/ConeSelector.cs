using System.Collections.Generic;
using System.Linq;
using TDC.Core.Utility;
using UnityEngine;
using UnityEngine.InputSystem;

namespace TDC.Spellcasting.Selectors
{
    public class ConeSelector : Selector
    {
        public class ConeSelectorSettings : SelectorSettings
        {
            public float Angle = 30;
            public float Radius = 5;
        }

        protected override SelectorSettings _Settings { get; set; } = new ConeSelectorSettings();
        public new ConeSelectorSettings Settings { 
            get => (ConeSelectorSettings)_Settings;
            set => _Settings = value;
        }

        [SerializeField] private Material _ConeMaterial;

        private List<Transform> _ConeObjects = new List<Transform>();

        private void CreateCone()
        {
            var cone = new GameObject("Cone Mesh Test");
            var filter = cone.AddComponent<MeshFilter>();
            cone.AddComponent<MeshRenderer>().material = _ConeMaterial;
            filter.mesh = SectorMeshGenerator.Build3D(Settings.Radius, Settings.Angle);
            _ConeObjects.Add(cone.transform);
        }
        
        protected override void OnSelectionStart()
        {
            CreateCone();
        }

        protected override void OnSelectionEnd()
        {
            foreach (Transform coneObject in _ConeObjects)
            {
                Destroy(coneObject.gameObject);
            }
            _ConeObjects.Clear();
        }

        protected override void RequestTargetAdd(Target toAdd)
        {
            Targets.Add(toAdd);
        }

        protected override void UpdateSelection()
        {
            Transform current = _ConeObjects.Last();
            Vector3 sourcePosition = Source.position;
            current.position = sourcePosition - new Vector3(0,0.1f,0);
            Ray ray = MainCamera.ScreenPointToRay(new Vector3(Mouse.current.position.x.ReadValue(),
                Mouse.current.position.y.ReadValue(), 0));
            var sourcePlane = new Plane(Vector3.up, sourcePosition);
            sourcePlane.Raycast(ray, out float hit);
            Vector3 hitPoint = ray.GetPoint(hit);
            Vector3 sourceToHit = (hitPoint - sourcePosition).normalized;
            current.forward = sourceToHit;
            LastSelectedTarget = new Target(sourceToHit);
        }
    }
}
