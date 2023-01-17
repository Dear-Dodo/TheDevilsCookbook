using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using TDC.Core.Utility;
using UnityEngine;
using UnityEngine.EventSystems;

namespace TDC.UI.Generic.Overlay
{
    /// <summary>
    /// Base class for all classes providing tooltips on hover.
    /// </summary>
    public abstract class TooltipProviderBase : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField, SerializedValueRequired]
        private TooltipProxyBase _TooltipPrefab;

        public TooltipProxyBase TooltipPrefab { get { return _TooltipPrefab; } set { _TooltipPrefab = value; } }

        [SerializeField] private float _AlphaTweenSpeed = 10.0f;

        protected TooltipProxyBase TooltipInstance;
        protected TweenerCore<float, float, FloatOptions> AlphaTweener;

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (TooltipInstance == null)
            {
                TooltipInstance = Instantiate(_TooltipPrefab, OverlayCanvas.Instance.transform);
                TooltipInstance.Group.alpha = 0;
                BuildTweener();
                OnTooltipCreated();
            }
            TooltipInstance.enabled = true;
            
            AlphaTweener.ChangeValues(TooltipInstance.Group.alpha, 1, _AlphaTweenSpeed);
            AlphaTweener.Restart();
            
            OnTooltipEnabled();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            TooltipInstance.enabled = false;
            AlphaTweener.ChangeValues(TooltipInstance.Group.alpha, 0, _AlphaTweenSpeed);
            AlphaTweener.Restart();
            OnTooltipDisabled();
        }

        protected abstract void OnTooltipCreated();
        protected abstract void OnTooltipEnabled();
        protected abstract void OnTooltipDisabled();

        protected virtual void Awake()
        {
            this.Validate();
        }

        private void BuildTweener()
        {
            AlphaTweener = DOTween.To(() => TooltipInstance.Group.alpha,
                value => TooltipInstance.Group.alpha = value,
                1, _AlphaTweenSpeed);
            AlphaTweener.SetSpeedBased(true);
            AlphaTweener.SetAutoKill(false);
            AlphaTweener.Pause();
        }

        protected void OnDestroy()
        {
            AlphaTweener?.Kill();
        }
    }
}
