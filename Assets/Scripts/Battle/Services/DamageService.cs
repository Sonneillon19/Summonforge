using UnityEngine;

namespace RPG.Battle
{
    public static class DamageService
    {
        public static int Compute(UnitRuntime caster, UnitRuntime target, SkillDef skill, bool crit)
        {
            float scale = skill.atkScale;
            float raw = (caster.StatsTotal.ATK * scale);
            float critMul = crit ? (1f + caster.StatsTotal.CritDmg) : 1f;
            float mitig = raw * (1000f / (1000f + Mathf.Max(target.StatsTotal.DEF, 0)));
            int result = Mathf.Max(0, Mathf.RoundToInt(mitig * critMul));
            return result;
        }
    }
}
