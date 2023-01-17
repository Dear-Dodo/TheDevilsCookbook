using UnityEngine;
using UnityEngine.Serialization;

namespace TDC.UI.HUD.Minimap
{
    public class IconObject : MonoBehaviour
    {
        public Texture2D Icon;
        public Vector3 IconLocalOffset = Vector3.zero;
        [FormerlySerializedAs("IconLocalSize")] public Vector3 Size = Vector3.one;


        private void OnDrawGizmosSelected()
        {
            Transform objTransform = transform;
            Quaternion rotation = objTransform.rotation;
            Vector3 worldOffset = rotation * IconLocalOffset;
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(objTransform.position + worldOffset, Size);
        }
    }
}
