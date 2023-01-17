using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using TDC.Core.Manager;
using TDC.Core.Utility;
using UnityAsync;
using UnityEngine;
using UnityEngine.UI;

namespace TDC.UI.HUD.Health
{
    public class HealthWidget : MonoBehaviour, IHideable
    {
        [SerializeField, SerializedValueRequired]
        private Image _MaggiePortrait;

        [SerializeField, Tooltip("Portraits for Maggie's health states. Spread evenly over her health cap.")]
        private Sprite[] _MaggieHealthStates;

        [SerializeField]
        private Sprite _MaggieDamageState;

        [SerializeField] private Sprite _MaggieDeadState;

        [SerializeField] private float _DamagePortraitTime = 0.25f;

        [SerializeField]
        private int _HealthOffset = 0;

        [Header("Bar Settings")]
        [SerializeField, SerializedValueRequired]
        private GameObject _SeparatorPrefab;

        [SerializeField, SerializedValueRequired]
        private RectTransform _SeparatorContainer;

        [SerializeField, SerializedValueRequired]
        private Image _CurrentHealthBar;

        [SerializeField, SerializedValueRequired]
        private Image _GhostHealthBar;

        [SerializeField] private float _BarTweenSpeed = 1;
        [SerializeField] private float _GhostTweenDelay = 0.25f;

        [SerializeField] private float _BarThickness = 10.0f;
        [SerializeField] private float _RadialOffset = 50.0f;

        private readonly List<GameObject> _Separators = new List<GameObject>();

        private TweenerCore<float, float, FloatOptions> _GhostTweener;

        private CancellationTokenSource _DamagePortraitTokenSource = new CancellationTokenSource();

        private async void Start()
        {
            this.Validate();
            await GameManager.LevelInitialisedAsync.WaitAsync();
            CreateSeparators();
            (await GameManager.PlayerCharacter.GetPlayerStats()).Health.OnValueChanged += OnHealthChanged;
            _GhostTweener = DOTween.To(() => _GhostHealthBar.fillAmount, value => _GhostHealthBar.fillAmount = value,
                _GhostHealthBar.fillAmount, _BarTweenSpeed);
            _GhostTweener.SetAutoKill(false);
            _GhostTweener.Pause();
        }

        private void OnValidate()
        {
            if (!Application.isPlaying || !GameManager.LevelInitialisedAsync.IsSet) return;
            RebuildSeparators();
        }

        private async void OnHealthChanged(int oldValue, int newValue)
        {
            float fillAmount = newValue / (float)(await GameManager.PlayerCharacter.GetPlayerStats()).MaxHealth / 2f + 0.5f;
            _CurrentHealthBar.fillAmount = fillAmount;
            if (fillAmount > _GhostHealthBar.fillAmount)
            {
                _GhostTweener.Pause();
                _GhostHealthBar.fillAmount = fillAmount;
                return;
            }
            _DamagePortraitTokenSource.Cancel();
            _DamagePortraitTokenSource = new CancellationTokenSource();

            _GhostTweener.ChangeValues(_GhostHealthBar.fillAmount, fillAmount, _BarTweenSpeed);
            _GhostTweener.Restart(true, _GhostTweenDelay);

            _MaggiePortrait.sprite = _MaggieDamageState;
            CancellationToken token = _DamagePortraitTokenSource.Token;
            await Await.Seconds(_DamagePortraitTime).ConfigureAwait(token);
            token.ThrowIfCancellationRequested();
            _MaggiePortrait.sprite = await GetPortraitSprite(newValue);
        }

        private async Task<Sprite> GetPortraitSprite(int currentHealth)
        {
            float segmentCount = (await GameManager.PlayerCharacter.GetPlayerStats()).MaxHealth;
            if (currentHealth <= 0)
            {
                return _MaggieDeadState;
            }
            return _MaggieHealthStates[Mathf.Clamp(
                Mathf.FloorToInt((currentHealth + _HealthOffset) / segmentCount * _MaggieHealthStates.Length),
                0, _MaggieHealthStates.Length - 1)];
        }

        public void RebuildSeparators()
        {
            foreach (GameObject separator in _Separators)
            {
                Destroy(separator);
            }
            _Separators.Clear();
            CreateSeparators();
        }

        /// <summary>
        /// Create health segment separators using polar to cartesian coordinate conversions.
        /// </summary>
        private async void CreateSeparators()
        {
            if (GameManager.PlayerCharacter == null) return;

            int segmentCount = (await GameManager.PlayerCharacter.GetPlayerStats()).MaxHealth;
            float segmentGap = Mathf.PI / segmentCount;
            for (var i = 1; i < segmentCount; i++)
            {
                float theta = segmentGap * i;
                float radius = 1 - _RadialOffset;
                Vector2 cartesianMax = (new Vector2(radius * Mathf.Cos(theta), radius * Mathf.Sin(theta)) + Vector2.one) / 2.0f;
                Vector2 cartesianMin = cartesianMax;
                cartesianMin.y -= _BarThickness / 2.0f;
                var rectTransform = Instantiate(_SeparatorPrefab, _SeparatorContainer).GetComponent<RectTransform>();
                rectTransform.pivot = new Vector2(0.5f, 1.0f);
                rectTransform.Rotate(0, 0, Mathf.Rad2Deg * (theta - Mathf.PI / 2));
                rectTransform.anchorMin = cartesianMin;
                rectTransform.anchorMax = cartesianMax;
                rectTransform.offsetMin = new Vector2(rectTransform.offsetMin.x, 0);
                rectTransform.offsetMax = new Vector2(rectTransform.offsetMax.x, 0);

                _Separators.Add(rectTransform.gameObject);
            }
        }

        private async void OnDestroy()
        {
            (await GameManager.PlayerCharacter.GetPlayerStats()).Health.OnValueChanged -= OnHealthChanged;
            _GhostTweener?.Kill();
        }

        public void SetHidden(bool isHidden)
        {
            gameObject.SetActive(!isHidden);
        }
    }
}