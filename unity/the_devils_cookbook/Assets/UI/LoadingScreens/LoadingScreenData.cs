using UnityEngine;

namespace TDC.UI.LoadingScreens
{
    [CreateAssetMenu(menuName = "TDC/Level/Loading Screen Data")]
    public class LoadingScreenData : ScriptableObject
    {
        [SerializeField] public Sprite[] LoadingScreenPool;
    }
}