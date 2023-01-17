using System;
using System.Collections.Generic;
using TDC.Core.Utility;
using UnityEditor;
using UnityEngine;

namespace TDC.Cooking
{
#if UNITY_EDITOR

    [CustomEditor(typeof(Recipe))]
    public class RecipeEditor : Editor
    {
        private string[] Names;
        private Type[] AllAppliences;

        public void OnEnable()
        {
            AllAppliences = AppDomain.CurrentDomain.GetAllDerivedTypes(typeof(Appliance));
            Names = Array.ConvertAll(AllAppliences, x => x.Name);
        }

        public override void OnInspectorGUI()
        {
            Recipe targetRecipe = (Recipe)target;
            DrawDefaultInspector();

            if (Names.Length != 0)
            {
                targetRecipe.Mask = EditorGUILayout.MaskField(targetRecipe.Mask, Names);
                List<Type> Types = new List<Type>();
                for (int i = 0; i < AllAppliences.Length; i++)
                {
                    Type ApplienceType = AllAppliences[i];
                    if ((targetRecipe.Mask & (1 << i)) == (1 << i))
                    {
                        Types.Add(ApplienceType);
                    }
                }
                targetRecipe.Appliances = Types.ToArray();
                // EditorUtility.SetDirty(target);
            }
            else
            {
                EditorGUILayout.HelpBox("Warning: No Appliances Avaliable", MessageType.Warning);
            }
        }

        public override Texture2D RenderStaticPreview(string assetPath, UnityEngine.Object[] subAssets, int width, int height)
        {
            Recipe targetRecipe = (Recipe)target;

            Texture2D newIcon = new Texture2D(width, height);


            if (targetRecipe.Output[0].Icon.texture != null)
            {
                EditorUtility.CopySerialized(targetRecipe.Output[0].Icon.texture, newIcon);
                return newIcon;
            }

            return base.RenderStaticPreview(assetPath, subAssets, width, height);
        }
    }

#endif

    [CreateAssetMenu(fileName = "Recipe", menuName = "TDC/Cooking/Recipe")]
    public class Recipe : ScriptableObject
    {
        public Food[] Input;
        public Food[] Output;
        public Modifier ProcessTimeModifier;
        public float ProcessTime;
        public Difficulty Difficulty;

        [HideInInspector]
        public Type[] Appliances;

        [HideInInspector]
        public int Mask;
    }
}