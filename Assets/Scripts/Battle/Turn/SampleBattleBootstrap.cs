using System.Collections.Generic;
using UnityEngine;

namespace RPG.Battle
{
    // Construye las unidades en runtime usando SOs y las asigna al TurnManager
    public class SampleBattleBootstrap : MonoBehaviour
    {
        public TurnManager turnManager;
        public UnitDef unitA;
        public UnitDef unitB;
        public SkillDef basicSkill;

        private void Awake()
        {
            var u1 = new UnitRuntime { Def = unitA, StatsBase = unitA.baseStats, StatsTotal = unitA.baseStats };
            u1.Skills.Add(new SkillRuntime { Def = basicSkill });

            var u2 = new UnitRuntime { Def = unitB, StatsBase = unitB.baseStats, StatsTotal = unitB.baseStats };
            u2.Skills.Add(new SkillRuntime { Def = basicSkill });

            var party = new List<UnitRuntime> { u1, u2 };
            turnManager.SetUnits(party);
        }
    }
}
