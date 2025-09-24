using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPG.Battle
{
    public class TurnManager : MonoBehaviour
    {
        [Range(0.001f, 0.1f)] public float atbFactor = 0.01f;

        // NUEVO: referencia al input (así el HUD puede fijar objetivo)
        public BattleInput input;

        private readonly List<UnitRuntime> _units = new();
        public IReadOnlyList<UnitRuntime> Units => _units;

        public event Action OnEnemiesDefeated;
        public event Action OnPlayersDefeated;
        public event Action UnitsChanged;

        public void SetUnits(List<UnitRuntime> list)
        {
            _units.Clear();
            if (list != null) _units.AddRange(list);
            UnitsChanged?.Invoke();
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            foreach (var u in _units) u.Tick(dt, atbFactor);

            var acting = _units.FirstOrDefault(u => u.Atb >= 1f && !u.IsDead);
            if (acting != null)
            {
                ExecuteTurn(acting);
                CheckTeamsStatus();
            }
        }

        private void ExecuteTurn(UnitRuntime u)
        {
            var skill = u.Skills.FirstOrDefault(s => s.Ready);
            u.StartTurn();

            if (skill == null)
            {
                u.ResetAtb();
                return;
            }

            // Elegir objetivo
            UnitRuntime target = null;

            // Si es jugador y hay selección válida, úsala
            if (u.Team == Team.Player && input != null && input.IsValidEnemyTarget(input.SelectedTarget, Team.Player))
                target = input.SelectedTarget;

            // Si no hay seleccion válida, toma el primer enemigo vivo
            if (target == null)
                target = _units.FirstOrDefault(x => x.Team != u.Team && !x.IsDead);

            var targets = new List<UnitRuntime>();
            if (target != null) targets.Add(target);
            if (targets.Count == 0) { u.ResetAtb(); return; }

            skill.Use(u, targets);
            u.EndTurn();
            u.ResetAtb();

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
    }
}
