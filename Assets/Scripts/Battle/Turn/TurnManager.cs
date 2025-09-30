using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RPG.Combat;   // CombatUnit / ApplyTiming

namespace RPG.Battle
{
    public enum BattleFlow { ATB, ManualTurns }

    public class TurnManager : MonoBehaviour
    {
        [Header("Modo de batalla")]
        public BattleFlow flow = BattleFlow.ManualTurns;

        [Header("ATB")]
        [Range(0.001f, 0.1f)]
        public float atbFactor = 0.01f;

        [Header("Input existente (selección objetivo)")]
        public BattleInput input;

        private readonly List<UnitRuntime> _units = new();
        public IReadOnlyList<UnitRuntime> Units => _units;

        public event Action OnEnemiesDefeated;
        public event Action OnPlayersDefeated;
        public event Action UnitsChanged;
        public event Action<UnitRuntime> OnPlayerTurnBegan;

        private PendingAction _awaitedAction;
        private UnitRuntime _current;
        private Coroutine _manualLoop;

        public class PendingAction
        {
            public UnitRuntime user;
            public int skillIndex;
            public List<UnitRuntime> targets;
        }

        private CombatUnit CU(UnitRuntime u) => u != null ? u.Combat : null;

        private UnitRuntime EnsureRuntimeBound(CombatUnit cu)
        {
            if (cu == null) return null;
            var rt = new UnitRuntime();
            rt.BindCombat(cu);
            return rt;
        }

        public void RegisterCombatUnit(CombatUnit cu)
        {
            var rt = EnsureRuntimeBound(cu);
            if (rt != null && !_units.Contains(rt))
            {
                _units.Add(rt);
                UnitsChanged?.Invoke();
            }
        }

        private void PruneInvalid()
        {
            for (int i = _units.Count - 1; i >= 0; i--)
            {
                var rt = _units[i];
                if (rt == null || rt.Combat == null || rt.Combat.gameObject == null)
                    _units.RemoveAt(i);
            }
        }

        public void SetUnits(List<UnitRuntime> list)
        {
            _current = null;
            _awaitedAction = null;
            _units.Clear();

            if (list != null)
            {
                foreach (var u in list)
                {
                    if (u == null || u.Combat == null) continue;
                    _units.Add(u);
                }
            }

            foreach (var u in _units)
                u?.ResetAtb();

            PruneInvalid();
            UnitsChanged?.Invoke();

            if (flow == BattleFlow.ManualTurns)
            {
                if (_manualLoop != null) StopCoroutine(_manualLoop);
                _manualLoop = StartCoroutine(ManualTurnLoop());
            }
        }

        public void SetUnitsFromCombats(IEnumerable<CombatUnit> combats)
        {
            _current = null;
            _awaitedAction = null;
            _units.Clear();

            if (combats != null)
            {
                foreach (var cu in combats)
                {
                    var rt = EnsureRuntimeBound(cu);
                    if (rt != null) _units.Add(rt);
                }
            }

            foreach (var u in _units)
                u?.ResetAtb();

            PruneInvalid();
            UnitsChanged?.Invoke();

            if (flow == BattleFlow.ManualTurns)
            {
                if (_manualLoop != null) StopCoroutine(_manualLoop);
                _manualLoop = StartCoroutine(ManualTurnLoop());
            }
        }

        private void Start()
        {
            if (flow == BattleFlow.ManualTurns)
                _manualLoop = StartCoroutine(ManualTurnLoop());
        }

        private void Update()
        {
            if (flow == BattleFlow.ATB)
            {
                float dt = Time.deltaTime;
                foreach (var u in _units)
                    if (!u.IsDead && u.Combat != null)
                        u.Tick(dt, atbFactor);

                var acting = _units.FirstOrDefault(u => u.Combat != null && u.Atb >= 1f && !u.IsDead);
                if (acting != null)
                {
                    ExecuteTurn_ATB(acting);
                    CheckTeamsStatus();
                }
            }

            if (flow == BattleFlow.ManualTurns && _manualLoop == null)
                _manualLoop = StartCoroutine(ManualTurnLoop());
        }

        // =====================================================================
        //                      MODO MANUAL: ATB por SALTOS
        // =====================================================================
        private IEnumerator ManualTurnLoop()
        {
            while (true)
            {
                PruneInvalid();

                if (_units.Count == 0)
                {
                    yield return null;
                    continue;
                }

                float stepDt = ComputeTimeToNextActingUnit();
                if (stepDt > 0f)
                {
                    foreach (var u in _units)
                        if (!u.IsDead && u.Combat != null) u.Tick(stepDt, atbFactor);
                }
                else
                {
                    if (!_units.Any(u => u.Combat != null && !u.IsDead && u.Atb >= 1f))
                        yield return null;
                }

                _current = _units
                    .Where(u => u.Combat != null && !u.IsDead && u.Atb >= 1f)
                    .OrderByDescending(u => u.Atb)
                    .FirstOrDefault();

                if (_current == null)
                {
                    yield return null;
                    continue;
                }

                if (_current.Combat == null)
                {
                    Debug.LogWarning("[Turn] UnitRuntime sin CombatUnit al iniciar turno.");
                    yield return null;
                    continue;
                }

                var cu = CU(_current);
                cu?.Status?.Tick(ApplyTiming.PerTurnStart);
                _current.StartTurn();

                if (cu && cu.Status.IsActionPrevented())
                {
                    yield return ResolveEndTurn(_current);
                    yield return null;
                    continue;
                }

                if (_current.Team == Team.Player)
                {
                    _awaitedAction = null;
                    OnPlayerTurnBegan?.Invoke(_current);

                    while (_awaitedAction == null)
                        yield return null;

                    ExecuteSkillByIndex(_awaitedAction.user, _awaitedAction.skillIndex, _awaitedAction.targets);
                }
                else
                {
                    int skillIndex = SelectEnemySkillIndex(_current);
                    var target = FirstAliveEnemy(_current.Team);
                    if (target != null)
                        ExecuteSkillByIndex(_current, skillIndex, new List<UnitRuntime> { target });
                }

                yield return ResolveEndTurn(_current);
                yield return null;
            }
        }

        // =====================================================================
        //                         Utilidades de Turnos
        // =====================================================================
        private float ComputeTimeToNextActingUnit()
        {
            const float MIN_RATE = 0.0001f;
            const float MIN_STEP = 0.01f;
            const float EPSILON = 0.0001f;

            float best = float.PositiveInfinity;

            foreach (var u in _units)
            {
                if (u.IsDead || u.Combat == null) continue;

                if (u.Atb >= 1f - EPSILON)
                    return MIN_STEP;

                float rate = Mathf.Max(GetSpeed(u) * atbFactor, MIN_RATE);
                float dt = (1f - u.Atb) / rate;

                if (dt > 0f && dt < best)
                    best = dt;
            }

            if (float.IsPositiveInfinity(best)) return MIN_STEP;
            return Mathf.Max(best, MIN_STEP);
        }

        private int SelectEnemySkillIndex(UnitRuntime enemy)
        {
            for (int i = 1; i < enemy.Skills.Count; i++)
            {
                var s = enemy.Skills[i];
                if (s != null && SkillCooldownAdapter.GetReady(s))
                    return i;
            }
            return 0;
        }

        public void SubmitPlayerAction(int skillIndex, List<UnitRuntime> targets)
        {
            if (flow != BattleFlow.ManualTurns) return;
            if (_current == null || _current.Team != Team.Player) return;
            if (_current.Combat == null) return;

            if (targets == null || targets.Count == 0)
            {
                var fallback = FirstAliveEnemy(_current.Team);
                if (fallback != null)
                {
                    targets = new List<UnitRuntime> { fallback };
                }
                else
                {
                    Debug.LogWarning("[Submit] No valid targets!");
                    return;
                }
            }

            _awaitedAction = new PendingAction
            {
                user = _current,
                skillIndex = Mathf.Clamp(skillIndex, 0, _current.Skills.Count - 1),
                targets = targets
            };
        }

        private void ExecuteSkillByIndex(UnitRuntime user, int skillIndex, List<UnitRuntime> targets)
        {
            if (user == null || user.Combat == null) return;
            if (skillIndex < 0 || skillIndex >= user.Skills.Count) return;
            var skill = user.Skills[skillIndex];
            if (skill == null) return;

            skill.Use(user, targets);

            if (skillIndex > 0)
                SkillCooldownAdapter.StartCooldown(skill);
        }

        private IEnumerator ResolveEndTurn(UnitRuntime u)
        {
            var cu = CU(u);
            cu?.Status?.Tick(ApplyTiming.PerTurnEnd);

            for (int i = 1; i < u.Skills.Count; i++)
                if (u.Skills[i] != null)
                    SkillCooldownAdapter.ReduceCooldown(u.Skills[i], 1);

            u.EndTurn();
            u.ResetAtb();

            float dt = ComputeTimeToNextActingUnit();
            if (dt <= 0f) dt = 0.01f;

            foreach (var other in _units)
            {
                if (other == null || other.IsDead || other.Combat == null) continue;
                if (other == u) continue;
                other.Tick(dt, atbFactor);
            }

            _current = null;
            _awaitedAction = null;

            CheckTeamsStatus();
            yield return null;
        }

        private UnitRuntime FirstAliveEnemy(Team myTeam)
        {
            return _units.FirstOrDefault(x => x != null && x.Combat != null && x.Team != myTeam && !x.IsDead);
        }

        // ============================== MODO ATB (auto) ==============================
        private void ExecuteTurn_ATB(UnitRuntime u)
        {
            if (u == null || u.IsDead || u.Combat == null) return;

            var cu = CU(u);
            cu?.Status?.Tick(ApplyTiming.PerTurnStart);

            int idx = SelectEnemySkillIndex(u);
            var skill = u.Skills[idx];
            u.StartTurn();

            if (cu && cu.Status.IsActionPrevented())
            {
                u.EndTurn();
                u.ResetAtb();
                cu.Status.Tick(ApplyTiming.PerTurnEnd);
                return;
            }

            UnitRuntime target = null;
            if (u.Team == Team.Player && input != null && input.IsValidEnemyTarget(input.SelectedTarget, Team.Player))
                target = input.SelectedTarget;
            if (target == null) target = FirstAliveEnemy(u.Team);

            if (skill != null && target != null)
            {
                skill.Use(u, new List<UnitRuntime> { target });
                if (idx > 0) SkillCooldownAdapter.StartCooldown(skill);
            }

            cu?.Status?.Tick(ApplyTiming.PerTurnEnd);

            for (int i = 1; i < u.Skills.Count; i++)
                if (u.Skills[i] != null)
                    SkillCooldownAdapter.ReduceCooldown(u.Skills[i], 1);

            u.EndTurn();
            u.ResetAtb();

            if (input != null && target != null && target.IsDead)
                input.ClearIf(target);
        }

        private void CheckTeamsStatus()
        {
            int enemiesAlive = _units.Count(u => u.Team == Team.Enemy && u.Combat != null && !u.IsDead);
            int playersAlive = _units.Count(u => u.Team == Team.Player && u.Combat != null && !u.IsDead);

            if (enemiesAlive == 0) OnEnemiesDefeated?.Invoke();
            if (playersAlive == 0) OnPlayersDefeated?.Invoke();
        }

        private float GetSpeed(UnitRuntime u)
        {
            const float DEFAULT_SPD = 100f;

            var cu = CU(u);
            if (cu == null) return DEFAULT_SPD;

            var t = cu.GetType();
            object val = null;

            var p = t.GetProperty("Speed") ?? t.GetProperty("speed") ?? t.GetProperty("SPD");
            if (p != null)
            {
                val = p.GetValue(cu);
                if (val is float f1) return f1;
                if (val is int i1) return i1;
            }

            var f = t.GetField("Speed") ?? t.GetField("speed") ?? t.GetField("SPD");
            if (f != null)
            {
                val = f.GetValue(cu);
                if (val is float f2) return f2;
                if (val is int i2) return i2;
            }

            var pStats = t.GetProperty("Stats") ?? t.GetProperty("stats");
            if (pStats != null)
            {
                var stats = pStats.GetValue(cu);
                if (stats != null)
                {
                    var ts = stats.GetType();
                    var ps = ts.GetProperty("Speed") ?? ts.GetProperty("speed") ?? ts.GetProperty("SPD");
                    if (ps != null)
                    {
                        val = ps.GetValue(stats);
                        if (val is float f3) return f3;
                        if (val is int i3) return i3;
                    }
                    var fs = ts.GetField("Speed") ?? ts.GetField("speed") ?? ts.GetField("SPD");
                    if (fs != null)
                    {
                        val = fs.GetValue(stats);
                        if (val is float f4) return f4;
                        if (val is int i4) return i4;
                    }
                }
            }

            return DEFAULT_SPD;
        }
    }
}
