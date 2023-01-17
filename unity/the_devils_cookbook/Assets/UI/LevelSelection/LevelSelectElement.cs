using TDC.Core.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TDC.UI.Menu.LevelSelect
{
    public class LevelSelectElement : MonoBehaviour
    {
        public Image IconImage;

        private LevelEntry _Entry;

        public void Initialize(LevelEntry entry)
        {
            _Entry = entry;
            IconImage.sprite = entry.Data.Icon;
        }
    }
}