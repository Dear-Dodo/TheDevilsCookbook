using UnityEngine;

namespace TDC.Prompts
{
    [CreateAssetMenu(fileName = "Prompt", menuName = "TDC/Prompts/Entry")]
    public class PromptData : ScriptableObject
    {
        [SerializeField, TextArea(1, 5)] public string Content;
        [SerializeField] public Vector3 ChevronOffset;
    }
}