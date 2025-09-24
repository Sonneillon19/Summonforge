using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RPG.Battle
{
    public class StageController : MonoBehaviour
    {
        [Header("Refs")]
        public TurnManager turnManager;
        public StageDef stage;

        [Header("Equipo del jugador (elige tus UnitDef)")]
        public UnitDef[] playerTeam;

        [Header("Opciones")]
        public float nextWaveDelay = 0.8f;

        private int _waveIndex = -1;
        private bool _advancing;

        private void Awake()
        {
            // Suscribir a eventos del TurnManager
            turnManager.OnEnemiesDefeated += HandleEnemiesDefeated;
            turnManager.OnPlayersDefeated += HandlePlayersDefeated;
        }

        private void Start()
        {
            LoadNextWave();
        }

        private void HandleEnemiesDefeated()
        {
            if (!_advancing) StartCoroutine(AdvanceWaveCo());
        }

        private IEnumerator AdvanceWaveCo()
        {
            _advancing = true;
            yield return new WaitForSeconds(nextWaveDelay);
            LoadNextWave();
            _advancing = false;
        }

        private void HandlePlayersDefeated()
        {
            Debug.LogWarning("Derrota: todos los aliados han caído.");
            // Aquí podrías reiniciar stage, mostrar UI, etc.
        }

        private void LoadNextWave()
        {
            _waveIndex++;
            if (stage == null || stage.waves == null || _waveIndex >= stage.waves.Length)
            {
                Debug.Log("<color=green>¡Stage completado! Todas las oleadas vencidas.</color>");
                // Aquí podrías dar recompensas, transicionar de escena, etc.
                return;
            }

            var wave = stage.waves[_waveIndex];
            var list = new List<UnitRuntime>();

            // 1) Construir party del jugador
            foreach (var p in playerTeam.Where(p => p != null))
                list.Add(CreateRuntimeFromDef(p, Team.Player));

            // 2) Spawns de la oleada (enemigos)
            foreach (var spawn in wave.spawns)
            {
                if (spawn.unit == null || spawn.count <= 0) continue;
                for (int i = 0; i < spawn.count; i++)
                    list.Add(CreateRuntimeFromDef(spawn.unit, spawn.team));
            }

            turnManager.SetUnits(list);
            Debug.Log($"<b>Wave {_waveIndex + 1}/{stage.waves.Length}</b> cargada. Aliados: {list.Count(u=>u.Team==Team.Player)} | Enemigos: {list.Count(u=>u.Team==Team.Enemy)}");
        }

        private UnitRuntime CreateRuntimeFromDef(UnitDef def, Team team)
        {
            var u = new UnitRuntime
            {
                Def = def,
                Team = team,
                StatsBase = def.baseStats,
                StatsTotal = def.baseStats
            };

            // Copiar skills del SO a runtimes
            if (def.skills != null)
            {
                foreach (var s in def.skills)
                {
                    if (s == null) continue;
                    u.Skills.Add(new SkillRuntime { Def = s });
                }
            }

            return u;
        }
    }
}