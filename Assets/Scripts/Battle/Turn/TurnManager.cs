using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPG.Battle
{
    public class TurnManager : MonoBehaviour
    {
        [Range(0.001f, 0.1f)] public float atbFactor = 0.01f;

        private readonly List<UnitRuntime> _units = new();
        public IReadOnlyList<UnitRuntime> Units => _units;

        // Eventos para que el controlador de stage avance oleadas
        public event Action OnEnemiesDefeated;
        public event Action OnPlayersDefeated;
        public event System.Action UnitsChanged;
        public void SetUnits(List<UnitRuntime> list)
        {
            _units.Clear();
            if (list != null) _units.AddRange(list);
            UnitsChanged?.Invoke(); // <-- notifica al HUD
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            foreach (var u in _units) u.Tick(dt, atbFactor);

            var acting = _units.FirstOrDefault(u => u.Atb >= 1f && !u.IsDead);
            if (acting != null)
            {
                ExecuteTurn(acting);

                // tras cada acción, evalúa estado de equipos
                CheckTeamsStatus();
            }
        }

        private void ExecuteTurn(UnitRuntime u)
        {
            var skill = u.Skills.FirstOrDefault(s => s.Ready);

            u.StartTurn(); // baja CD antes de decidir

            if (skill == null)
            {
                // Sin skill lista → pasar turno
                u.ResetAtb();
                return;
            }

            var targets = _units.Where(x => x.Team != u.Team && !x.IsDead).Take(1).ToList();
            if (targets.Count == 0) { u.ResetAtb(); return; }

            skill.Use(u, targets);
            u.EndTurn();
            u.ResetAtb();

            // Log de estado para depurar avance de oleadas
            int aliveEnemies = _units.Count(x => x.Team == Team.Enemy && !x.IsDead);
            Debug.Log($"[PostTurn] Enemigos vivos: {aliveEnemies}");
        }

        private void CheckTeamsStatus()
        {
            int enemiesAlive = _units.Count(u => u.Team == Team.Enemy && !u.IsDead);
            int playersAlive = _units.Count(u => u.Team == Team.Player && !u.IsDead);

            bool enemiesAllDead = enemiesAlive == 0;
            bool playersAllDead = playersAlive == 0;

            if (enemiesAllDead)
            {
                Debug.Log("<color=yellow>OnEnemiesDefeated()</color>");
                OnEnemiesDefeated?.Invoke();
            }
            if (playersAllDead)
            {
                Debug.Log("<color=red>OnPlayersDefeated()</color>");
                OnPlayersDefeated?.Invoke();
            }
        }
    }
}
