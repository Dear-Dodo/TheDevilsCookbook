using System;
using TDC.Core.Type;
using TDC.Core.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace TDC.UI.Generic
{
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class TMPHyperlinkHandler : MonoBehaviour, IPointerClickHandler, IApplicationStartHandler
    {
        private TextMeshProUGUI _TextMesh;
        [SerializeField] private Color32 _LinkColour = new Color32(0, 127, 238, 255);
        [SerializeField] private Color32 _LinkHoverColour = new Color32(0, 182, 238, 255);
        [FormerlySerializedAs("ParseOnAwake")] [SerializeField] private bool ParseOnApplicationStart = false;
        
        private int _HoveredLinkIndex = -1;

        public void ApplicationStart()
        {
            // _TextMesh = GetComponent<TextMeshProUGUI>();
            // if (ParseOnApplicationStart)
            // {
            //     _TextMesh.text = TMPTextParser.Parse(_TextMesh.text, new TMPTextParser.ParserSettings());
            //     _TextMesh.ForceMeshUpdate(true, true);
            // }
        }

        private void Awake()
        {
            _TextMesh = GetComponent<TextMeshProUGUI>();
            if (ParseOnApplicationStart)
            {
                string text =TMPTextParser.Parse(_TextMesh.text, new TMPTextParser.ParserSettings());
                _TextMesh.text = text;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Vector3 pos = eventData.position;
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(_TextMesh, pos, null);
            if (linkIndex < 0) return;
            TMP_LinkInfo linkInfo = _TextMesh.textInfo.linkInfo[linkIndex];
            Application.OpenURL(linkInfo.GetLinkID());
        }

        private void SetLinkColour(TMP_LinkInfo link, Color colour)
        {
            int firstIndex = link.linkTextfirstCharacterIndex;
            int lastIndex = firstIndex + link.linkTextLength - 1;
            TMP_CharacterInfo firstChar = _TextMesh.textInfo.characterInfo[firstIndex];
            TMP_CharacterInfo lastChar = _TextMesh.textInfo.characterInfo[lastIndex];

            int lineCount = lastChar.lineNumber - firstChar.lineNumber + 1;
            
            int underlineIndexStart = firstChar.underlineVertexIndex;
            Color32[] underlineVerts = _TextMesh.textInfo.meshInfo[0].colors32;
            for (int v = underlineIndexStart; v < underlineIndexStart + 12 * lineCount; v++)
            {
                underlineVerts[v] = colour;
            }
            
            for (int i = firstIndex; i < firstIndex + link.linkTextLength; i++)
            {
                TMP_CharacterInfo charInfo = _TextMesh.textInfo.characterInfo[i];
                if (!charInfo.isVisible) continue;
                int matIndex = charInfo.materialReferenceIndex;
                int vertIndex = charInfo.vertexIndex;
                int underlineIndex = charInfo.underlineVertexIndex;

                Color32[] vertexColors = _TextMesh.textInfo.meshInfo[matIndex].colors32;
                
                for (var j = 0; j < 4; j++)
                {
                    vertexColors[vertIndex + j] = colour;
                    // underlineVerts[underlineIndex + j] = colour;
                }
            }
        }
        
        private void UpdatePointerHover()
        {
            Vector3 mpos = Mouse.current.position.ReadValue();
            int linkIndex = TMP_TextUtilities.FindIntersectingLink(_TextMesh, mpos, null);
            if (linkIndex == _HoveredLinkIndex) return;
            
            if (_HoveredLinkIndex != -1)
            {
                SetLinkColour(_TextMesh.textInfo.linkInfo[_HoveredLinkIndex], _LinkColour);
            }
            if (linkIndex != -1)
            {
                SetLinkColour(_TextMesh.textInfo.linkInfo[linkIndex], _LinkHoverColour);
            }
            _HoveredLinkIndex = linkIndex;
            _TextMesh.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }

        private void Update()
        {
            UpdatePointerHover();
        }
    }
}
