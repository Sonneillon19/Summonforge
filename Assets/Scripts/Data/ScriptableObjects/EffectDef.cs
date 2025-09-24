using UnityEngine;

namespace RPG.Battle
{
    [CreateAssetMenu(fileName = "EffectDef", menuName = "RPG/EffectDef")]
    public class EffectDef : ScriptableObject
    {
        public string effectId;
        public string displayName;
        public bool isBuff;
        public int maxStacks = 1;
    }
}
