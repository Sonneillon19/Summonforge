using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RPG.Combat;
using RPG.Bridge; // <-- para CombatUnit / ApplyTiming

namespace RPG.Battle
{
    public class TurnManager : MonoBehaviour
    {
        [Header("Bridge de estados (config)")]
        public RPG.Bridge.StatusBridgeConfig bridgeConfig;
        [Range(0.001f, 0.1f)]
        public float atbFactor = 0.01f;
        // Referencia al input existente (para selección de objetivo)
        public BattleInput input;

        private readonly List<UnitRuntime> _units = new();
        public IReadOnlyList<UnitRuntime> Units => _units;

        public event Action OnEnemiesDefeated;
        public event Action OnPlayersDefeated;
        public event Action UnitsChanged;

        // ===== Helpers de integración con CombatUnit/Status =====
        private CombatUnit CU(UnitRuntime u) => u != null ? u.Combat : null;

        public void SetUnits(List<UnitRuntime> list)
        {
            _units.Clear();
            if (list != null) _units.AddRange(list);
            UnitsChanged?.Invoke();
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            foreach (var u in _units)
                u.Tick(dt, atbFactor);

            var acting = _units.FirstOrDefault(u => u.Atb >= 1f && !u.IsDead);
            if (acting != null)
            {
                ExecuteTurn(acting);
                CheckTeamsStatus();
            }
        }

        private void ExecuteTurn(UnitRuntime u)
        {
            if (u == null || u.IsDead) return;

            var cu = CU(u); // CombatUnit (puede ser null si no lo agregaste al prefab)

            // === INICIO DE TURNO: Tick de estados ===
            if (cu && cu.Status != null)
                cu.Status.Tick(ApplyTiming.PerTurnStart);

            var skill = u.Skills.FirstOrDefault(s => s.Ready);
            u.StartTurn();

            // Si la unidad está controlada (stun/freeze/etc), salta su acción
            if (cu && cu.Status != null && cu.Status.IsActionPrevented())
            {
                Debug.Log($"[Turn] {UName(u)} está controlado → salta acción.");
                u.EndTurn();
                u.ResetAtb();

                // === FIN DE TURNO (aunque no actuó): Tick de estados ===
                cu.Status.Tick(ApplyTiming.PerTurnEnd);
                return;
            }

            if (skill == null)
            {
                // No tiene skill lista: solo consume turno
                u.EndTurn();
                u.ResetAtb();

                if (cu && cu.Status != null)
                    cu.Status.Tick(ApplyTiming.PerTurnEnd);
                return;
            }

            // ====== Elegir objetivo ======
            UnitRuntime target = null;

            // Si es jugador y hay selección válida, úsala
            if (u.Team == Team.Player && input != null && input.IsValidEnemyTarget(input.SelectedTarget, Team.Player))
                target = input.SelectedTarget;

            // Si no hay selección válida, toma el primer enemigo vivo
            if (target == null)
                target = _units.FirstOrDefault(x => x.Team != u.Team && !x.IsDead);

            var targets = new List<UnitRuntime>();
            if (target != null) targets.Add(target);

            if (targets.Count == 0)
            {
                u.EndTurn();
                u.ResetAtb();

                if (cu && cu.Status != null)
                    cu.Status.Tick(ApplyTiming.PerTurnEnd);
                return;
            }

            // === Ejecutar skill (tu sistema actual) ===
            skill.Use(u, targets);
            RPG.Bridge.StatusBridge.TryApplyByKey(skill, u, targets, bridgeConfig);

            u.EndTurn();
            u.ResetAtb();

            // === FIN DE TURNO: Tick de estados (DoT / reducción de duración) ===
            if (cu && cu.Status != null)
                cu.Status.Tick(ApplyTiming.PerTurnEnd);

            int aliveEnemies = _units.Count(x => x.Team == Team.Enemy && !x.IsDead);
            Debug.Log($"[PostTurn] Enemigos vivos: {aliveEnemies}");

            // Si el objetivo murió, limpia la selección
            if (input != null && target != null && target.IsDead)
                input.ClearIf(target);
        }

        private void CheckTeamsStatus()
        {
            int enemiesAlive = _units.Count(u => u.Team == Team.Enemy && !u.IsDead);
            int playersAlive = _units.Count(u => u.Team == Team.Player && !u.IsDead);

            bool enemiesAllDead = enemiesAlive == 0;
            bool playersAllDead = playersAlive == 0;

            if (enemiesAllDead) OnEnemiesDefeated?.Invoke();
            if (playersAllDead) OnPlayersDefeated?.Invoke();
        }

        //Helper
        private string UName(UnitRuntime u)
        {
            var cu = CU(u);
            if (cu != null && !string.IsNullOrEmpty(cu.displayName))
                return cu.displayName;
            // Fallbacks por si tienes algo en UnitRuntime:
            // if (!string.IsNullOrEmpty(u.Name)) return u.Name;
            return "(Unit)";
        }
        // Dev/QA: terminar el turno del actor actual (ATB >= 1)
        public void EndTurn()
        {
            var acting = _units.FirstOrDefault(u => u.Atb >= 1f && !u.IsDead);
            if (acting == null) return;

            var cu = CU(acting); // helper que ya añadimos antes: private CombatUnit CU(UnitRuntime u) => u?.Combat;
            if (cu && cu.Status != null)
                cu.Status.Tick(ApplyTiming.PerTurnEnd); // tick de fin

            acting.EndTurn();   // tu método existente en UnitRuntime
            acting.ResetAtb();  // consume el turno

            // NO llamamos a ExecuteTurn aquí; tu Update() ya buscará al siguiente cuando llegue a ATB>=1
        }
    }
}
