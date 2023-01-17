using DG.Tweening;
using TDC.Core.Manager;
using UnityEngine;
using UnityEngine.UI;

namespace TDC.UI.Intro
{
    public class IntroCutsceneUI : MonoBehaviour
    {
        public Image IntroImage;
        public float Time;

        public async void Start()
        {
            await GameManager.LevelInitialisedAsync.WaitAsync();
            GameManager.LevelIntroCutscene.OnTargetSet += OnTargetSet;
            GameManager.LevelIntroCutscene.OnGo += OnGo;
            GameManager.LevelIntroCutscene.OnStart += Enable;
            GameManager.LevelIntroCutscene.OnEnd += Disable;
            Disable();
        }

        public void OnDestroy()
        {
            if (!Application.isPlaying) return;
            GameManager.LevelIntroCutscene.OnTargetSet -= OnTargetSet;
            GameManager.LevelIntroCutscene.OnGo -= OnGo;
            GameManager.LevelIntroCutscene.OnStart -= Enable;
            GameManager.LevelIntroCutscene.OnEnd -= Disable;
        }

        public void OnTargetSet(Sprite sprite)
        {
            IntroImage.transform.localScale = new Vector3();
            IntroImage.sprite = sprite;
            IntroImage.transform.DOScale(new Vector3(1, 1, 1), Time);
        }

        public void OnGo(Sprite sprite)
        {
            IntroImage.transform.localScale = new Vector3();
            IntroImage.sprite = sprite;
            IntroImage.transform.DOScale(new Vector3(1, 1, 1), Time);
        }

        private void Disable() => gameObject.SetActive(false);

        private void Enable() => gameObject.SetActive(true);
    }
}