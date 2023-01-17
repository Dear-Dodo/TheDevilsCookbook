using TDC.Core.Manager;
using UnityEngine;
using UnityEngine.VFX;

namespace TDC
{
    public class VFXtester : MonoBehaviour
    {
        public VisualEffect vfx;
        // Start is called before the first frame update
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
            if (GameManager.PlayerControls.Player.Interact.WasPressedThisFrame())
            {
                vfx.Play();
            }
        }
    }
}
