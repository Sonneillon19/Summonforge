using UnityEngine;
using RPG.Combat;

namespace RPG.Bridge
{
    [CreateAssetMenu(menuName = "RPG/Bridge/StatusBridgeConfig")]
    public class StatusBridgeConfig : ScriptableObject
    {
        [System.Serializable]
        public class Mapping
        {
            [Tooltip("Clave de la skill (Id/Nombre/Tipo). Debe coincidir con lo que resuelva el Bridge.")]
            public string skillKey;

            [Tooltip("Aplica sobre Self (lanzador) o Targets (objetivos).")]
            public ApplyTo applyTo = ApplyTo.Targets;

            [Range(0,100)] public int chance = 100;
            public int durationTurns = 2;
            public int stacks = 1;
            public StatusEffectSO[] effects;
        }

        public enum ApplyTo { Targets, Self }

        public Mapping[] mappings;
    }
}
