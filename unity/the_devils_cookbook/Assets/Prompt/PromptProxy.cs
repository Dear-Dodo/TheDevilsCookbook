using UnityEngine;

namespace TDC.Prompts
{
    public class PromptProxy : MonoBehaviour
    {
        public GameObject Target;
        public PromptData LinkedData;

        public PromptManager Manager;

        public void Awake()
        {
            Manager = FindObjectOfType<PromptManager>();
        }

        public void Activate()
        {
            Manager.Open(LinkedData, Target);
        }
    }
}