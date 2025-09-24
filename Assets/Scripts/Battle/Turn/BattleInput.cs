using System;
using UnityEngine;

namespace RPG.Battle
{
    /// Estado de selección de objetivo compartido.
    public class BattleInput : MonoBehaviour
    {
        public event Action<UnitRuntime> OnTargetChanged;

        private UnitRuntime _selected;
        public UnitRuntime SelectedTarget
        {
            get => _selected;
            private set
            {
                if (_selected == value) return;
                _selected = value;
                OnTargetChanged?.Invoke(_selected);
            }
        }

        public void SetTarget(UnitRuntime u)
        {
            SelectedTarget = u;
        }

        public bool IsValidEnemyTarget(UnitRuntime u, Team playerTeam)
        {
            return u != null && !u.IsDead && u.Team != playerTeam;
        }

        public void ClearIf(UnitRuntime u)
        {
            if (_selected == u) SelectedTarget = null;
        }
    }
}
