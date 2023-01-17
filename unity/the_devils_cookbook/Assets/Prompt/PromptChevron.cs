using UnityEngine;

namespace TDC.Prompts
{
    public class PromptChevron : MonoBehaviour
    {
        public GameObject Target;
        public Vector3 Offset;

        public void Update()
        {
            if (Target == null) return;
            transform.position = Target.transform.position + Offset;
        }
    }
}