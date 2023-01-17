using UnityEngine;
using UnityEngine.UI;

namespace TDC.UI.LoadingScreens
{
    public class ImageRandomizer : MonoBehaviour
    {
        [SerializeField] public Image ImageComponent;

        [SerializeField] public LoadingScreenData Data;

        [SerializeField] public bool PlayOnStart = true;

        public void Start()
        {
            if (PlayOnStart)
                ImageComponent.sprite = Data.LoadingScreenPool[Random.Range(0, Data.LoadingScreenPool.Length)];
        }
    }
}