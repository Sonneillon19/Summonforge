using System;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Battle
{
    [Serializable]
    public class SkillRuntime
    {
        public SkillDef Def;
        public int Cooldown;

        public bool Ready => Cooldown <= 0;

        public void Use(UnitRuntime caster, List<UnitRuntime> targets)
        {
            foreach (var t in targets)
            {
                bool crit = false; // por ahora
                int dmg = DamageService.Compute(caster, t, Def, crit);
                t.StatsTotal.HP -= dmg;
                if (t.StatsTotal.HP < 0) t.StatsTotal.HP = 0; // clamp para consistencia visual
                Debug.Log($"{caster.Def.displayName} usa {Def.displayName} sobre {t.Def.displayName}: -{dmg} HP (resta {t.StatsTotal.HP})");
            }
                // Si cdBase==0, no impongas cooldown
        if (Def.cdBase > 0) Cooldown = Def.cdBase;
        }

        public void TickCooldown() { if (Cooldown > 0) Cooldown--; }
    }
}
