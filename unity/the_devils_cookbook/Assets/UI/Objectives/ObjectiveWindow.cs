using System.Collections.Generic;
using System.Threading.Tasks;
using DG.Tweening;
using TDC.UI.Windowing;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace TDC.UI.Objectives
{
    public class ObjectiveWindow : Window
    {
        [SerializeField] private ObjectiveWidget _WidgetPrefab;
        [FormerlySerializedAs("ObjectiveContainer")] [SerializeField] private RectTransform _ObjectiveContainer;
        [SerializeField] private TextMeshProUGUI _TitleText;

        private CanvasGroup _ObjectivesGroup;

        [SerializeField] private Color _IncompleteColour = new Color(0.8f,0.8f,0.8f,1);
        public Color IncompleteColour => _IncompleteColour;
        [SerializeField] private Color _CompleteColour = Color.white;
        public Color CompleteColour => _CompleteColour;
        [SerializeField] private Color _FailedColour = new Color(0.8f,0,0,1);

        [SerializeField] private float _ObjectiveFadeTime = 1.0f;

        private RectTransform _RectTransform;

        private bool _IsFadingAll;
        private bool _IsDirty = false;

        private ObjectiveManager _ObjectiveManager;

        private readonly List<ObjectiveWidget> _ObjectiveWidgets = new List<ObjectiveWidget>();

        private void Update()
        {
            if (!_IsDirty) return;
            LayoutRebuilder.MarkLayoutForRebuild(_ObjectiveContainer);
            _IsDirty = false;
        }

        private void Awake()
        {
            _ObjectiveManager = FindObjectOfType<ObjectiveManager>();
            _RectTransform = GetComponent<RectTransform>();
            if (!_ObjectiveManager)
            {
                Debug.LogError("No ObjectiveManager found in scene, Objective UI will not work correctly.");
                return;
            }

            _ObjectiveManager.ObjectivesCleared += FadeAndClearObjectives;
            _ObjectiveManager.ObjectiveAdded += RegisterObjective;
            _TitleText.color = _IncompleteColour;
            _ObjectivesGroup = _ObjectiveContainer.GetComponent<CanvasGroup>();
        }

        public void RegisterObjective(Objective objective)
        {
            ObjectiveWidget widget = Instantiate(_WidgetPrefab, _ObjectiveContainer);
            widget.Initialise(this, objective);
            _ObjectiveWidgets.Add(widget);
            // objective.Completed += (_) => OnObjectiveCompleted(widget);
            if (_IsFadingAll) widget.gameObject.SetActive(false);
            else _IsDirty = true;
        }
        //
        // private void OnObjectiveCompleted(ObjectiveWidget widget)
        // {
        //     widget.ObjectiveText.fontStyle |= FontStyles.Strikethrough;
        //     widget.ObjectiveText.color = _CompleteColour;
        // }
        
        public void ClearObjectives()
        {
            foreach (ObjectiveWidget widget in _ObjectiveWidgets)
            {
                Destroy(widget);
            }
            _ObjectiveWidgets.Clear();
        }

        public void FadeAndClearObjectives()
        {
            _IsFadingAll = true;
            var widgetsToClear = new List<ObjectiveWidget>(_ObjectiveWidgets);
            _ObjectiveWidgets.Clear();
            Sequence fadeSequence = DOTween.Sequence();
            fadeSequence.Append(_ObjectivesGroup.DOFade(0, _ObjectiveFadeTime));
            fadeSequence.AppendCallback(() => FinishObjectiveFade(widgetsToClear));
            fadeSequence.Play();
        }

        private void FinishObjectiveFade(IEnumerable<ObjectiveWidget> widgets)
        {
            foreach (ObjectiveWidget widget in widgets)
            {
                Destroy(widget.gameObject);
            }
            _ObjectivesGroup.alpha = 1;
            _IsFadingAll = false;
            foreach (ObjectiveWidget widget in _ObjectiveWidgets)
            {
                widget.gameObject.SetActive(true);
            }

            _IsDirty = true;
        }
        
        public override Task OnOpen(bool shouldPlayAnimation)
        {
            return Task.CompletedTask;
        }

        public override Task<bool> OnClose(bool shouldPlayAnimation)
        {
            return Task.FromResult(true);
        }
    }
}
