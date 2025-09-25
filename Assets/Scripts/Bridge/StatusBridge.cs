using System;
using System.Collections.Generic;
using UnityEngine;
using RPG.Combat;
using RPG.Battle;

namespace RPG.Bridge
{
    public static class StatusBridge
    {
        // Intenta resolver una "clave" para la skill (Id, Name, Type.Name)
        public static string ResolveSkillKey(object skillObj)
        {
            if (skillObj == null) return string.Empty;
            var t = skillObj.GetType();

            // Busca propiedades comunes por reflexión (no rompe tu sistema actual)
            var idProp = t.GetProperty("Id") ?? t.GetProperty("ID") ?? t.GetProperty("SkillId") ?? t.GetProperty("skillId");
            if (idProp != null)
            {
                var val = idProp.GetValue(skillObj) as string;
                if (!string.IsNullOrEmpty(val)) return val;
            }

            var nameProp = t.GetProperty("Name") ?? t.GetProperty("DisplayName") ?? t.GetProperty("Nombre");
            if (nameProp != null)
            {
                var val = nameProp.GetValue(skillObj) as string;
                if (!string.IsNullOrEmpty(val)) return val;
            }

            // Fallback: el nombre del tipo
            return t.Name;
        }

        // Llama esto después de skill.Use(...)
        public static void TryApplyByKey(
            object skillObj,
            UnitRuntime user,
            List<UnitRuntime> targets,
            StatusBridgeConfig config)
        {
            if (config == null) return;

            string key = ResolveSkillKey(skillObj);
            if (string.IsNullOrEmpty(key)) return;

            foreach (var map in config.mappings)
            {
                if (map == null || map.effects == null) continue;
                if (!string.Equals(map.skillKey, key, StringComparison.Ordinal)) continue;

                // Aplica a Self o Targets
                if (map.applyTo == StatusBridgeConfig.ApplyTo.Self)
                {
                    ApplyEffectsToUnit(user, user, map);
                }
                else // Targets
                {
                    if (targets != null)
                        foreach (var t in targets)
                            ApplyEffectsToUnit(user, t, map);
                }
            }
        }

        private static void ApplyEffectsToUnit(UnitRuntime user, UnitRuntime target, StatusBridgeConfig.Mapping map)
        {
            if (user?.Combat == null || target?.Combat == null) return;

            // Probabilidad
            if (map.chance < 100)
            {
                int roll = UnityEngine.Random.Range(0, 100);
                if (roll >= map.chance) return;
            }

            foreach (var so in map.effects)
            {
                if (so == null) continue;
                target.Combat.Status.Apply(so, map.durationTurns, map.stacks, user.Combat);
                Debug.Log($"[Bridge] {ResolveName(user)} aplica {so.displayName} a {ResolveName(target)}");
            }
        }

        private static string ResolveName(UnitRuntime u)
        {
            var cu = u?.Combat;
            return (cu != null && !string.IsNullOrEmpty(cu.displayName)) ? cu.displayName : "(Unit)";
        }
    }
}
