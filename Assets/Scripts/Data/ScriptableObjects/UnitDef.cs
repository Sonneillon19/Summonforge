using UnityEngine;

namespace RPG.Battle
{
    [CreateAssetMenu(fileName = "UnitDef", menuName = "RPG/UnitDef")]
    public class UnitDef : ScriptableObject
    {
        public string unitId;
        public string displayName;
        public int rarity;
        public string role;
        public Stats baseStats;
        public SkillDef[] skills;
    }
}
