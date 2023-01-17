using UnityEngine;
using UnityEngine.UI;

namespace TDC.UI.Generic
{
    public class TimerUI : MonoBehaviour
    {
        public Image FillImage;

        public float MaxTime;
        public bool Acending;

        private float _CurrentTime;
        private bool _Active;

        public void StartTimer() => _Active = true;

        public void StopTimer() => _Active = false;

        public void ResetTimer() => _CurrentTime = MaxTime;

        public void StartNew(bool setActive) => StartNew(MaxTime, setActive);

        public void StartNew(float maxTime, bool setActive = false)
        {
            MaxTime = maxTime;
            ResetTimer();
            StartTimer();
            gameObject.SetActive(setActive);
        }

        private void Update()
        {
            if (_Active && _CurrentTime > 0)
            {
                _CurrentTime -= Time.deltaTime;
                FillImage.fillAmount =
                    Acending ? (1.0f / MaxTime) * _CurrentTime : 1.0f - ((1.0f / MaxTime) * _CurrentTime);
                return;
            }
            _CurrentTime = 0;
        }
    }
}