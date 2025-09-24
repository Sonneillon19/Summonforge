using UnityEngine;

namespace RPG.Battle
{
    [CreateAssetMenu(fileName = "StageDef", menuName = "RPG/StageDef")]
    public class StageDef : ScriptableObject
    {
        [System.Serializable]
        public struct UnitSpawnDef
        {
            public UnitDef unit;
            public int level;
            public int count;
            public Team team; // Player o Enemy (normalmente Enemy para oleadas)
        }

        [System.Serializable]
        public struct Wave
        {
            public UnitSpawnDef[] spawns;
        }

        public Wave[] waves;
    }
}
