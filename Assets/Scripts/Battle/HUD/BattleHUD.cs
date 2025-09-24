using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace RPG.Battle
{
    public class BattleHUD : MonoBehaviour
    {
        public TurnManager turnManager;
        public Transform listParent;          // el Panel con VerticalLayoutGroup
        public UnitHUDRow rowPrefab;          // el prefab de la fila

        private void OnEnable()
        {
            if (turnManager != null)
                turnManager.UnitsChanged += Rebuild;
        }

        private void OnDisable()
        {
            if (turnManager != null)
                turnManager.UnitsChanged -= Rebuild;
        }

        private void Start()
        {
            Rebuild(); // por si ya hay Units asignadas
        }

        public void Rebuild()
        {
            Canvas.ForceUpdateCanvases();
            var rt = listParent as RectTransform;
            if (rt != null) LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
            
            if (turnManager == null || listParent == null || rowPrefab == null) return;

            // limpia
            for (int i = listParent.childCount - 1; i >= 0; i--)
                Destroy(listParent.GetChild(i).gameObject);

            // crea filas: primero aliados, luego enemigos
            var ordered = turnManager.Units
                .OrderBy(u => u.Team == Team.Enemy) // Player (false) primero, Enemy (true) después
                .ToList();

            foreach (var u in ordered)
            {
                var row = Instantiate(rowPrefab, listParent);
                row.Bind(u);
            }
        }
    }
}
