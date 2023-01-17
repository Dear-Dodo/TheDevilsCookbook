using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using TDC.Core.Manager;
using TDC.UI.Windowing;
using UnityEngine;

namespace TDC.UI.Objectives
{
    public class ObjectiveManager : MonoBehaviour
    {
        private WindowNode _ObjectivesInstance;

        private List<Objective> _Objectives = new List<Objective>();
        public ReadOnlyCollection<Objective> Objectives => _Objectives.AsReadOnly();

        public event Action ObjectivesCleared;
        public event Action ObjectivesCompleted;
        public event Action<Objective> ObjectiveAdded;
        
        public async Task EnableObjectives()
        {
            _ObjectivesInstance = await GameManager.WindowManager.OpenAdditive("Objectives");
        }

        public void DisableObjectives()
        {
            _ = GameManager.WindowManager.Close(_ObjectivesInstance.WindowInstance);
        }

        public void AddObjective(Objective objective)
        {
            _Objectives.Add(objective);
            objective.Completed += CheckObjectives;
            ObjectiveAdded?.Invoke(objective);
        }

        public void ClearObjectives()
        {
            _Objectives.Clear();
            ObjectivesCleared?.Invoke();
        }

        private void CheckObjectives(bool shouldCheckCompletion)
        {
            if (!shouldCheckCompletion) return;
            if (_Objectives.TrueForAll(o => o.Status == ObjectiveStatus.Completed)) ObjectivesCompleted?.Invoke();
        }
    }
}
