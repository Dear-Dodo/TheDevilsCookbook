using UnityEngine;
using UnityEngine.UI;

namespace TDC.UI.Generic
{
    public class CentralGridLayout : LayoutGroup
    {
        [Min(1)]
        public int TargetRowCount = 2;
        public int ColumnLimit = 2;

        public Vector2 Spacing;

        protected float CellSize;

        // protected List<DrivenRectTransformTracker> Tracker = new List<DrivenRectTransformTracker>();

        protected void CalculateCellSize()
        {
            float availableSpace = rectTransform.rect.height - padding.vertical - (TargetRowCount - 1) * Spacing.y;
            CellSize = availableSpace / TargetRowCount;
        }
        
        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            CalculateCellSize();
            int columns = ColumnLimit > 0 ? Mathf.Min(rectChildren.Count, ColumnLimit) : rectChildren.Count;
            float minSize = padding.horizontal + CellSize * columns;
            SetLayoutInputForAxis(minSize, minSize, -1, 0);
        }
        
        public override void CalculateLayoutInputVertical()
        {
            float height = rectTransform.rect.height;
            SetLayoutInputForAxis(height, height, -1, 0);
        }

        // protected void SetChildrenOnAxis(int axis)
        // {
        //     int columnMaxElements = Mathf.CeilToInt((float)rectChildren.Count / (ColumnLimit > 0 ? ColumnLimit : 1));
        //     int rowMaxElements = ColumnLimit > 0 ? Mathf.Min(ColumnLimit, rectChildren.Count) : rectChildren.Count;
        //     
        //     for (var i = 0; i < rectChildren.Count; i += axis == 0 ? rowMaxElements : columnMaxElements)
        //     {
        //         int remainingChildren = rectChildren.Count - i;
        //         int elementsInSet = axis == 0 ? Mathf.Min(remainingChildren, rowMaxElements)
        //                 : Mathf.Min(columnMaxElements, remainingChildren);
        //         float offsetPerElement = (CellSize / 2.0f) + Spacing[axis];
        //         // Intentional integer division
        //         int startOffsetIndex = -(elementsInSet / 2);
        //         bool skipZero = axis == 0 ? (elementsInSet % 2 == 0) : columnMaxElements % 2 == 0;
        //         float centre = (axis == 0 ? rectTransform.rect.width : rectTransform.rect.height) / 2.0f;
        //         float startOffset = centre + startOffsetIndex * offsetPerElement;
        //         for (var j = 0; j < elementsInSet; j++)
        //         {
        //             int adjustedIndex = skipZero && startOffsetIndex + j >= 0 ? j + 1 : j;
        //             float offset = startOffset + adjustedIndex * offsetPerElement - (CellSize / 2.0f);
        //             SetChildAlongAxis(rectChildren[axis == 0 ? j + i : i + j * rowMaxElements], axis, offset, CellSize);
        //         }
        //
        //         // if (ColumnLimit == 0) break;
        //     }
        // }
        
        protected void SetChildrenOnAxis(int axis)
        {
            int columnMaxElements = Mathf.CeilToInt((float)rectChildren.Count / (ColumnLimit > 0 ? ColumnLimit : 1));
            int rowMaxElements = ColumnLimit > 0 ? Mathf.Min(ColumnLimit, rectChildren.Count) : rectChildren.Count;
            
            float offsetPerElement = (CellSize / 2.0f) + Spacing[axis];
            int maxIndex = axis == 0 ? rectChildren.Count : rowMaxElements;
            for (var i = 0; i < maxIndex; i += axis == 0 ? rowMaxElements : 1)
            {
                int remainingChildren = rectChildren.Count - i;
                int elementsInSet = axis == 0 ? Mathf.Min(remainingChildren, rowMaxElements)
                    : Mathf.Min(Mathf.CeilToInt((float)remainingChildren / rowMaxElements), columnMaxElements);
                // Intentional integer division
                int startOffsetIndex = -((axis == 0 ? elementsInSet : columnMaxElements) / 2);
                bool skipZero = axis == 0 ? (elementsInSet % 2 == 0) : columnMaxElements % 2 == 0;
                float centre = (axis == 0 ? rectTransform.rect.width - padding.right + padding.left 
                    : rectTransform.rect.height - padding.bottom + padding.top) / 2.0f;
                float startOffset = centre + startOffsetIndex * offsetPerElement;
                for (var j = 0; j < elementsInSet; j++)
                {
                    int adjustedIndex = skipZero && startOffsetIndex + j >= 0 ? j + 1 : j;
                    float offset = startOffset + adjustedIndex * offsetPerElement - (CellSize / 2.0f);
                    int targetIndex = axis == 0 ? j + i : i + j * rowMaxElements;
                    SetChildAlongAxis(rectChildren[targetIndex], axis, offset, CellSize);
                }

                
            }
        }
        
        public override void SetLayoutHorizontal()
        {
            SetChildrenOnAxis(0);
            /*
            for (var i = 0; i < rectChildren.Count; i += ColumnLimit)
            {
                int remainingChildren = rectChildren.Count - i;
                int elementsInRow = ColumnLimit > 0 ? Mathf.Min(remainingChildren, ColumnLimit) : remainingChildren;
                float offsetPerElement = (CellSize / 2.0f) + Spacing.x;
                // Intentional integer division
                int startOffsetIndex = -(elementsInRow / 2);
                bool skipZero = elementsInRow % 2 == 0;
                float startOffset = (rectTransform.rect.width / 2) + startOffsetIndex * offsetPerElement;
                for (int j = i; j < i + elementsInRow; j++)
                {
                    int adjustedIndex = skipZero && startOffsetIndex + j >= 0 ? j + 1 : j;
                    int offsetIndex = startOffsetIndex + adjustedIndex;
                    float offset = startOffset + adjustedIndex * offsetPerElement;
                    SetChildAlongAxis(rectChildren[j], 0, offset);
                }
            }
             */
        }

        public override void SetLayoutVertical()
        {
            SetChildrenOnAxis(1);
            // throw new System.NotImplementedException();
        }
    }
}