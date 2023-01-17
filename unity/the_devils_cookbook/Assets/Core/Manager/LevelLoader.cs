using DG.Tweening;
using FMODUnity;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TDC.Core.Extension;
using TDC.Core.Type;
using TDC.Level;
using TDC.UI.Menu.LevelSelect;
using UnityEngine;
using Object = UnityEngine.Object;

namespace TDC.Core.Manager
{
    [Serializable]
    public class LevelEntry : SceneEntry
    {
        public LevelData Data;
    }

    [Serializable]
    public class LevelLoader : GameManagerSubsystem
    {
        [SerializeField] public SceneEntry LoadingScene;
        [SerializeField] public ImageContainer FadeToBlackPrefab;
        [SerializeField] public float FadeToBlackTime;

        [SerializeField] private List<LevelEntry> _Levels;

        public ImageContainer FadeToBlackObject;

        public List<LevelEntry> Levels
        { get { return new List<LevelEntry>(_Levels); } }

        [SerializeField]
        public Range LoadTimeMs = new Range
        {
            Min = 2000,
            Max = 5000
        };

        public event Action<LevelEntry> onLevelLoadStart;

        public event Action<LevelEntry> onLevelLoadFinished;

        public ReadOnlyCollection<LevelEntry> GetLevels() => _Levels?.AsReadOnly();

        public async Task LoadLevel(int index)
        {
            if (_Levels.InBounds(index))
            {
                LevelEntry entry = _Levels[(int)index];

                RuntimeManager.GetBus("bus:/Master/SFX").stopAllEvents(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);

                onLevelLoadStart?.Invoke(entry);

                if (FadeToBlackObject)
                    Object.Destroy(FadeToBlackObject);
                FadeToBlackObject = Object.Instantiate(FadeToBlackPrefab, new Vector3(), Quaternion.identity);
                FadeToBlackObject.gameObject.SetActive(true);
                FadeToBlackObject.Image.color = new Color(0, 0, 0, 0);
                await FadeToBlackObject.Image.DOColor(new Color(0, 0, 0, 1), FadeToBlackTime).AsyncWaitForCompletion();

                FadeToBlackObject.gameObject.SetActive(false);

                await GameManager.SceneLoader.LoadScene(entry, LoadTimeMs.Random(), LoadingScene);
                GameManager.CurrentLevelData = entry.Data;

                onLevelLoadFinished?.Invoke(entry);
            }
        }

        protected override Task OnInitialise()
        {
            return Task.CompletedTask;
        }

        protected override void Reset()
        {
            throw new System.NotImplementedException();
        }
    }
}