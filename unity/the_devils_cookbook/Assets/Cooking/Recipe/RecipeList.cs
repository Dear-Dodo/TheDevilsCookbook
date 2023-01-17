using System.Collections.Generic;
using UnityEngine;
using TDC.Core.Extension;
using TDC.Core.Utility;
using System;

#if UNITY_EDITOR

using UnityEditor;

#endif

namespace TDC.Cooking
{
    //in editor find all recipies that use an applience and add them to the list

#if UNITY_EDITOR

    [CustomEditor(typeof(RecipeList))]
    public class RecipeListEditor : Editor
    {
        private string[] Names;
        private System.Type[] AllAppliences;

        public void OnEnable()
        {
            AllAppliences = System.AppDomain.CurrentDomain.GetAllDerivedTypes(typeof(Appliance));
            Names = Array.ConvertAll(AllAppliences, x => x.Name);
        }

        public override void OnInspectorGUI()
        {
            RecipeList targetRecipeList = (RecipeList)target;
            DrawDefaultInspector();
            targetRecipeList.typeIndex = EditorGUILayout.Popup(targetRecipeList.typeIndex, Names);
            targetRecipeList.ApplianceType = AllAppliences[targetRecipeList.typeIndex];
            updateRecipes(targetRecipeList);
        }

        private void updateRecipes(RecipeList targetRecipeList)
        {
            List<Recipe> recipes = ScriptableObjectEx.LoadAssets<Recipe>();
            List<Recipe> result = new List<Recipe>();
            if (recipes != null && recipes.Count > 0)
            {
                foreach (Recipe recipe in recipes)
                {
                    if (recipe.Appliances == null)
                    {
                        recipe.Mask = EditorGUILayout.MaskField(recipe.Mask, Names);
                        List<Type> Types = new List<Type>();
                        for (int i = 0; i < AllAppliences.Length; i++)
                        {
                            Type ApplienceType = AllAppliences[i];
                            if ((recipe.Mask & (1 << i)) == (1 << i))
                            {
                                Types.Add(ApplienceType);
                            }
                        }
                        recipe.Appliances = Types.ToArray();
                    }
                    if (recipe.Appliances.Length > 0)
                    {
                        foreach (System.Type applience in recipe.Appliances)
                        {
                            if (applience == targetRecipeList.ApplianceType)
                            {
                                result.Add(recipe);
                                break;
                            }
                        }
                    }
                }
                if (result.Count == 0)
                {
                    EditorGUILayout.HelpBox("Warning: Appliance Has No Recipies", MessageType.Warning);
                }
            }
            else
            {
                EditorGUILayout.HelpBox("Warning: No Recipies Found", MessageType.Warning);
            }
            targetRecipeList.Recipes = result.ToArray();
            EditorUtility.SetDirty(target);
        }
    }

#endif

    [CreateAssetMenu(fileName = "RecipeList", menuName = "TDC/Cooking/RecipeList")]
    public class RecipeList : ScriptableObject
    {
        public System.Type ApplianceType;
        public Recipe[] Recipes;

        [HideInInspector]
        public int typeIndex;
    }
}