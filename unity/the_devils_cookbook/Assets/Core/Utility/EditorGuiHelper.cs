using System;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace TDC.Core.Utility
{
    public static class EditorGuiHelper
    {
#if UNITY_EDITOR

        /// <summary>
        /// Draw Vector3 data to the editor.
        /// </summary>
        /// <param name="label">string param for data title</param>
        /// <param name="data">Vector3 reference to display data.</param>
        public static void Draw(string label, ref Vector3 data) => data = EditorGUILayout.Vector3Field(label, data);

        /// <summary>
        /// Draw Vector2 data to the editor.
        /// </summary>
        /// <param name="label">string param for data title</param>
        /// <param name="data">Vector2 reference to display data.</param>
        public static void Draw(string label, ref Vector2 data) => data = EditorGUILayout.Vector2Field(label, data);

        /// <summary>
        /// Draw Vector4 data to the editor.
        /// </summary>
        /// <param name="label">string param for data title</param>
        /// <param name="data">Vector4 reference to display data.</param>
        public static void Draw(string label, ref Vector4 data) => data = EditorGUILayout.Vector4Field(label, data);

        /// <summary>
        /// Draw string data to the editor.
        /// </summary>
        /// <param name="label">string param for data title</param>
        /// <param name="data">string reference to display data.</param>
        public static void Draw(string label, ref string data) => data = EditorGUILayout.TextField(label, data);

        /// <summary>
        /// Draw a button with actions to the editor.
        /// </summary>
        /// <param name="label">label of the button</param>
        /// <param name="mainDelegate">action if button is pressed.</param>
        /// <param name="elseDelegate">action if button is not pressed.</param>
        public static void DrawButton(string label, Action mainDelegate, Action elseDelegate = null)
        {
            if (GUILayout.Button(label))
            {
                mainDelegate();
            }
            else
            {
                elseDelegate?.Invoke();
            }
        }

#endif
    }
}