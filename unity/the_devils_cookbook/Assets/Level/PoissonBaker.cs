using TDC.Core.Utility.Samplers;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using Utility;
#if UNITY_EDITOR
namespace TDC.Level
{
    public class PoissonBaker : MonoBehaviour
    {
        private PoissonDisc _Disc;
        public LevelData BakeTarget;

        public float Size = 5;
        public float Spacing = 1;
        public float NavmeshLeniancy = 0.1f;
        public float GizmoSize = 0.1f;
        
        [SerializeField]
        private bool _Generate;

        private void OnValidate()
        {
            if (!_Generate) return;
            _Generate = false;
            Generate();
        }

        private void Generate()
        {
            bool NavmeshValidator(Vector2 v) => 
                NavMesh.SamplePosition(v.xoy(), out _, NavmeshLeniancy, NavMesh.AllAreas);
            _Disc = PoissonDisc.Generate(transform.position.xz(), 
                new Vector2(Size,Size), Spacing, 30, NavmeshValidator);
            BakeTarget.PoissonDisc = _Disc;
            EditorUtility.SetDirty(BakeTarget);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(transform.position, new Vector3(Size,0.1f,Size));
            if (BakeTarget?.PoissonDisc?.Points == null) return;
            Gizmos.color = Color.blue;
            foreach (Vector2 point in BakeTarget.PoissonDisc.Points)
            {
                Vector3 pos = point.xoy();
                Gizmos.DrawSphere(pos, GizmoSize);
                Gizmos.DrawWireSphere(pos, Spacing);
                Gizmos.color = Color.red;
            }
        }
    }
}
#endif