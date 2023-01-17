using TDC.Core.Manager;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

namespace TDC.Debugging
{
    public class DebugLog : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        public Log log;
        public Image Panel;
        public ColorBlock Colors;
        public int Count;
        public TextMeshProUGUI ConditionText;
        public TextMeshProUGUI StackTraceText;
        public TextMeshProUGUI CountText;
        private bool _MouseOver = false;
        private bool _Clicked = false;
        private bool _Selected = false;

        public void OnPointerDown(PointerEventData eventData)
        {
            OnPointerDown();
        }
        public void OnPointerDown()
        {
            _Clicked = _MouseOver;
            if (!_MouseOver) _Selected = false;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (_Clicked && _MouseOver)
            {
                _Selected = true;
            }
            _Clicked = false;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            _MouseOver = true;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            _MouseOver = false;
        }

        // Start is called before the first frame update
        void Start()
        {
            string logText = "";
            
            switch (log.Type)
            {
                case LogType.Error:
                    logText += "Error: ";
                    ConditionText.color = Color.red;
                    break;
                case LogType.Assert:
                    logText += "Assert: ";
                    ConditionText.color = Color.cyan;
                    break;
                case LogType.Warning:
                    logText += "Warning: ";
                    ConditionText.color = Color.yellow;
                    break;
                case LogType.Log:
                    logText += "Log: ";
                    ConditionText.color = Color.white;
                    break;
                case LogType.Exception:
                    logText += "Exception: ";
                    ConditionText.color = Color.red;
                    break;
            }
            logText += log.Condition;
            ConditionText.SetText(logText);
            GameManager.PlayerControls.UI.Click.performed += (context) => OnPointerDown();
        }

        // Update is called once per frame
        void Update()
        {
            if (_Selected)
            {
                Panel.color = Colors.selectedColor;
                StackTraceText.SetText(log.StackTrace);
                switch (log.Type)
                {
                    case LogType.Error:
                        StackTraceText.color = Color.red;
                        break;
                    case LogType.Assert:
                        StackTraceText.color = Color.cyan;
                        break;
                    case LogType.Warning:
                        StackTraceText.color = Color.yellow;
                        break;
                    case LogType.Log:
                        StackTraceText.color = Color.white;
                        break;
                    case LogType.Exception:
                        StackTraceText.color = Color.red;
                        break;
                }
                if (Count > 1)
                {
                    CountText.enabled = true;
                    CountText.text = Count.ToString();
                } else
                {
                    CountText.enabled = false;
                }
            }
            else
            {
                StackTraceText.color = Color.white;
                if (_Clicked)
                {
                    Panel.color = Colors.pressedColor;
                }
                else if (_MouseOver)
                {
                    Panel.color = Colors.highlightedColor;
                }
                else
                {
                    Panel.color = Colors.normalColor;
                }
            }
        }
    }
}
