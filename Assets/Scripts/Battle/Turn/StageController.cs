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

        [Header("Parents (opcional, para ordenar jerarquía)")]
        public Transform playerParent;
        public Transform enemyParent;

        [Header("Opciones")]
        public float nextWaveDelay = 0.8f;

        private int _waveIndex = -1;
        private bool _advancing;

        // llevamos el rastro de lo instanciado para limpiar entre waves
        private readonly List<RPG.Combat.CombatUnit> _spawnedCombats = new();

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

            // 0) limpiar lo instanciado en la wave anterior
            CleanupSpawned();

            var wave = stage.waves[_waveIndex];
            var runtimes = new List<UnitRuntime>();

            // 1) Construir party del jugador
            foreach (var p in playerTeam.Where(p => p != null))
                runtimes.Add(CreateRuntimeFromDef(p, Team.Player));

            // 2) Spawns de la oleada (enemigos)
            foreach (var spawn in wave.spawns)
            {
                if (spawn.unit == null || spawn.count <= 0) continue;
                for (int i = 0; i < spawn.count; i++)
                    runtimes.Add(CreateRuntimeFromDef(spawn.unit, spawn.team));
            }

            // IMPORTANTE: ahora los runtimes ya traen Combat enlazado
            turnManager.SetUnits(runtimes);

            Debug.Log(
                $"<b>Wave {_waveIndex + 1}/{stage.waves.Length}</b> cargada. " +
                $"Aliados: {runtimes.Count(u => u.Team == Team.Player)} | " +
                $"Enemigos: {runtimes.Count(u => u.Team == Team.Enemy)}");
        }

        /// <summary>
        /// Crea un UnitRuntime y LE ENLAZA un CombatUnit (instanciando prefab si existe).
        /// </summary>
        private UnitRuntime CreateRuntimeFromDef(UnitDef def, Team team)
        {
            var u = new UnitRuntime
            {
                Def = def,
                Team = team,
                StatsBase = def.baseStats,
                StatsTotal = def.baseStats
            };

            if (def.skills != null)
            {
                foreach (var s in def.skills)
                {
                    if (s == null) continue;
                    u.Skills.Add(new SkillRuntime { Def = s });
                }
            }

            // Enlazar CombatUnit creado en escena
            var cu = SpawnCombatFor(def, team);
            u.BindCombat(cu);

            return u;
        }
        /// <summary>
        /// Instancia un GameObject y devuelve su CombatUnit.
        /// - Si UnitDef tiene prefab, lo usa; si no, crea un GO vacío con CombatUnit.
        /// - Setea Team y displayName.
        /// - Lo cuelga del parent correspondiente si se asignó.
        /// </summary>
        private RPG.Combat.CombatUnit SpawnCombatFor(UnitDef def, Team team)
        {
            // Creamos un GO simple (sin prefab) y lo colgamos del parent si existe
            var parent = (team == Team.Player) ? playerParent : enemyParent;

            string niceName = def ? def.name : "Unit";
            var go = new GameObject($"[{team}] {niceName}");
            if (parent != null) go.transform.SetParent(parent, false);

            // Aseguramos el CombatUnit
            var cu = go.AddComponent<RPG.Combat.CombatUnit>();

            // Nombre a mostrar en HUD/UI (TurnManager usa cu.displayName)
            cu.displayName = niceName;

            // (Opcional) inicializar stats del Combat a partir de def.baseStats si quieres reflejarlos también en el Combat:
            // cu.Stats = new StatsBlock(def.baseStats);  // depende de tu ctor/copia

            _spawnedCombats.Add(cu);
            return cu;
        }
        private void CleanupSpawned()
        {
            for (int i = _spawnedCombats.Count - 1; i >= 0; i--)
            {
                var cu = _spawnedCombats[i];
                if (cu != null) Destroy(cu.gameObject);
            }
            _spawnedCombats.Clear();
        }
    }
}
