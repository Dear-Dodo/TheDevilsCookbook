using UnityEngine;
using UnityEngine.UI;

namespace TDC.UI.Generic
{
    public class UIOverlay : MonoBehaviour, IHideable
    {
        private RawImage Overlay;
        public Texture2D OverlayTexture;
        public float Fade;
        public float FadeSpeed;
        private float _OldFade;
        private float _FadeLerp;
        private Texture2D _DefaultTexture;

        private void Start()
        {
            Overlay = GetComponent<RawImage>();
            _OldFade = Fade;
            _DefaultTexture = OverlayTexture;
        }

        // Update is called once per frame
        void Update()
        {
            Overlay.texture = OverlayTexture;
            if (Fade != _OldFade)
            {
                Overlay.color = new Color(Overlay.color.r, Overlay.color.g, Overlay.color.b, Mathf.Lerp(_OldFade, Fade, _FadeLerp));
                if (_FadeLerp < 1)
                {
                    _FadeLerp += Time.unscaledDeltaTime * FadeSpeed;
                }
                else
                {
                    _FadeLerp = 1;
                    _OldFade = Fade;
                    _FadeLerp = 0;
                }
            }
        }

        public void SetHidden(bool isHidden)
        {
            gameObject.SetActive(!isHidden);
        }
    }
}
