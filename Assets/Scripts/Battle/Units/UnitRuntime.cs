using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Battle
{
    public class UnitRuntime
    {
        public UnitDef Def;
        public Stats StatsBase;
        public Stats StatsTotal;
        public List<EffectInstance> Effects = new();
        public List<SkillRuntime> Skills = new();
        public float Atb;
        public Team Team;
        public bool IsDead => StatsTotal.HP <= 0;

        public void Tick(float dt, float spdFactor)
        {
            Atb += Mathf.Max(0, StatsTotal.SPD) * spdFactor * dt;
            if (Atb > 1f) Atb = 1f;
        }

        public void ResetAtb() => Atb = 0f;
        public void StartTurn()
        {
            // Baja cooldowns al comienzo del turno
            foreach (var s in Skills) s.TickCooldown();
        }
        public void EndTurn() {}
    }
}
