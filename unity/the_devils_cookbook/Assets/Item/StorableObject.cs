using TDC.Core.Utility;
using UnityEditor;
using UnityEngine;

namespace TDC.Items
{
#if UNITY_EDITOR
    [CustomEditor(typeof(StorableObject),true)]
    public class StorableObjectEditor : Editor
    {
    public override Texture2D RenderStaticPreview(string assetPath, UnityEngine.Object[] subAssets, int width, int height)
    {
        StorableObject Target = (StorableObject)target;

        Texture2D newIcon = new Texture2D(width, height);


        if (Target.Icon.texture != null)
        {
            EditorUtility.CopySerialized(Target.Icon.texture, newIcon);
            return newIcon;
        }

        return base.RenderStaticPreview(assetPath, subAssets, width, height);
    }
}

#endif


public enum ItemTypes
    {
        Ingedient,
        OrderableFood
    }
    public class StorableObject : ScriptableObject
    {
        public string Name;
        public ItemTypes StorageTypes;
        [SerializedValueRequired]
        public Sprite Icon;
        [SerializedValueRequired]
        public Item Prefab;

        public Item CreateItem()
        {
            return Instantiate(Prefab.gameObject).GetComponent<Item>();
        }
    }
}
