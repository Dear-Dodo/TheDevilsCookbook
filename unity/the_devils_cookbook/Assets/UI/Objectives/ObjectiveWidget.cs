using TMPro;
using UnityEngine;

namespace TDC.UI.Objectives
{
    public class ObjectiveWidget : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI ObjectiveText;

        private Objective _Objective;
        private ObjectiveWindow _ParentWindow;

        public void Initialise(ObjectiveWindow parentWindow, Objective objective)
        {
            _ParentWindow = parentWindow;
            _Objective = objective;

            switch (objective)
            {
                case QuantifiableObjective qObj:
                    qObj.QuantityChanged += UpdateQuantifiable;
                    UpdateQuantifiable(qObj.CurrentQuantity);
                    break;
                case BooleanObjective bObj:
                    bObj.Completed += UpdateBoolean;
                    UpdateBoolean(false);
                    break;
            }
        }

        private void UpdateQuantifiable(float value)
        {
            var objective = (QuantifiableObjective)_Objective;
            ObjectiveText.text = $"{objective.Text} ({value} / {objective.TargetQuantity})";
            ObjectiveText.color = _Objective.Status == ObjectiveStatus.Completed
                ? _ParentWindow.CompleteColour
                : _ParentWindow.IncompleteColour;
        }

        private void UpdateBoolean(bool _)
        {
            ObjectiveText.text = _Objective.Text;
            ObjectiveText.color = _Objective.Status == ObjectiveStatus.Completed
                ? _ParentWindow.CompleteColour
                : _ParentWindow.IncompleteColour;
        }
    }
}
