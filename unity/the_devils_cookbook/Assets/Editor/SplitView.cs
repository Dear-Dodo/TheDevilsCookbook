using UnityEngine.UIElements;

namespace TDC.Editor
{
    public class SplitView : TwoPaneSplitView
    {
        public new class UxmlFactory : UxmlFactory<SplitView, TwoPaneSplitView.UxmlTraits>
        {
        }
    }
}