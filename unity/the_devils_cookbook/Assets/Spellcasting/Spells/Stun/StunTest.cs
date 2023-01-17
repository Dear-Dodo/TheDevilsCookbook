using System.Collections.Generic;
using TDC.Core.Manager;
using UnityEngine;
using UnityEngine.VFX;

namespace TDC
{
    public class StunTest : MonoBehaviour
    {
        [SerializeField]
        public List<VisualEffect> vfx = new List<VisualEffect>();
        // Start is called before the first frame update
        void Start()
        {
            foreach (VisualEffect effect in vfx)
            {
                effect.Stop();
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (GameManager.PlayerControls.Player.Fire.WasPressedThisFrame())
            {
                foreach (VisualEffect effect in vfx)
                {
                    effect.Play();
                }
            }
        }
    }
}
