using UnityEngine;

namespace RPG.Battle
{
    [CreateAssetMenu(fileName = "SkillDef", menuName = "RPG/SkillDef")]
    public class SkillDef : ScriptableObject
    {
        public string skillId;
        public string displayName;
        public int cdBase = 0;
        public float atkScale = 1.0f;
    }
}
