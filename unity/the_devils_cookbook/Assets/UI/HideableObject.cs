using UnityEngine;

namespace TDC.UI
{
    public class HideableObject : MonoBehaviour, IHideable
    {
        public void SetHidden(bool isHidden)
        {
            gameObject.SetActive(!isHidden);
        }
    }
}
