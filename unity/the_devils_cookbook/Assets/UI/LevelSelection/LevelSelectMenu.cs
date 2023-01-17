using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TDC.Core.Manager;
using TDC.UI.Generic;
using TDC.UI.Windowing;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TDC.UI.Menu.LevelSelect
{
    public class LevelSelectMenu : Window
    {
        public LevelSelectElement TemplateElement;

        public SnappableScrollRect ScrollRect;

        public TextMeshProUGUI LevelNameText;

        public Button PlayButton;

        private int _CurrentIndex = 0;

        private RectTransform _Container;

        private List<GameObject> _Elements;

        private LevelEntry[] Levels
        {
            get => _Levels;
            set
            {
                if (ScrollRect)
                {
                    CreateData(value);
                }
            }
        }

        private LevelEntry[] _Levels;

        public async void Start()
        {
            if (ScrollRect != null)
            {
                ScrollRect.Initialize();
                ScrollRect.onElementChanged += ScrollRect_onElementChanged;
                await Initialize();
                _Container = ScrollRect.ScrollRect.content;
                CreateData(_Levels);
                PlayButton?.onClick?.AddListener(async () =>
                {
                    await GameManager.StartLevel(_CurrentIndex);
                });
            }
        }

      
        private void CreateData(LevelEntry[] levels)
        {
            _Levels = levels;

            _Elements ??= new List<GameObject>();
            _Elements.Clear();

            for (int i = 0; i < _Container.childCount; i++)
            {
                Transform child = _Container.GetChild(i);
                Destroy(child.gameObject);
            }

            foreach (LevelEntry level in Levels)
            {
                var element = Instantiate(TemplateElement.gameObject, _Container.transform).GetComponent<LevelSelectElement>();
                element.Initialize(level);
                _Elements.Add(element.gameObject);
            }

            ScrollRect.SetData(_Elements);
        }

        private async Task Initialize()
        {
            await GameManager.InitialisedAsync.WaitAsync();
            _Levels = GameManager.LevelLoader.GetLevels().ToArray();
        }

        private void ScrollRect_onElementChanged(int previous, int current)
        {
            if (_Levels != null && _Levels.Length > 0)
            {
                var index = Mathf.Clamp(current, 0, _Levels.Length);
                LevelNameText.text = _Levels[index].Data.LevelName;
                _CurrentIndex = index;
            }
        }

        public override Task OnOpen(bool shouldPlayAnimation)
        {
            throw new System.NotImplementedException();
        }

        public override Task<bool> OnClose(bool shouldPlayAnimation)
        {
            throw new System.NotImplementedException();
        }
    }
}