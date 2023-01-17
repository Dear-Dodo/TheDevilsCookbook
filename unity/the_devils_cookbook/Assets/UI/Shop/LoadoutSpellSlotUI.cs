using UnityEngine;
using UnityEngine.UI;

namespace TDC
{
    public class LoadoutSpellSlotUI : MonoBehaviour
    {
        public int Index;
        public bool Parent;
        public LoadoutSpell Spell;
        public Image SpellImage;

        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            if (SpellImage != null && Spell != null)
            {
                SpellImage.sprite = Spell.Image.sprite;
            }
        }
    }
}
