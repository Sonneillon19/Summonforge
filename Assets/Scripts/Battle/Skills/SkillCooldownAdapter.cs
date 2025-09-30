using System;
using System.Collections.Generic;
using System.Reflection;

namespace RPG.Battle
{
    public static class SkillCooldownAdapter
    {
        // Mapa interno para skills que NO tienen campos/props de cooldown
        private static readonly Dictionary<object, int> _remainingMap = new Dictionary<object, int>(ReferenceEqualityComparer.Instance);

        // === Lectores: intentan propiedades/campos estándar ===
        public static int GetMax(object skill)
        {
            if (skill == null) return 0;
            var t = skill.GetType();
            var p = t.GetProperty("CooldownTurns") ?? t.GetProperty("cooldownTurns");
            if (p != null && p.PropertyType == typeof(int)) return (int)p.GetValue(skill);
            var f = t.GetField("CooldownTurns") ?? t.GetField("cooldownTurns");
            if (f != null && f.FieldType == typeof(int)) return (int)f.GetValue(skill);
            return 0; // si no existe, interpretamos “sin CD fijo”
        }

        public static int GetRemaining(object skill)
        {
            if (skill == null) return 0;
            var t = skill.GetType();
            // 1) Propiedad/campo específico
            var p = t.GetProperty("CooldownTurnsRemaining") ?? t.GetProperty("cooldownRemaining");
            if (p != null && p.PropertyType == typeof(int)) return (int)p.GetValue(skill);
            var f = t.GetField("CooldownTurnsRemaining") ?? t.GetField("cooldownRemaining");
            if (f != null && f.FieldType == typeof(int)) return (int)f.GetValue(skill);

            // 2) Cache interno
            if (_remainingMap.TryGetValue(skill, out var val)) return val;

            // 3) Ready sólo lectura (no asumimos setter)
            var pr = t.GetProperty("Ready");
            if (pr != null && pr.PropertyType == typeof(bool))
                return ((bool)pr.GetValue(skill)) ? 0 : 1;

            return 0;
        }

        public static bool GetReady(object skill)
        {
            if (skill == null) return false;
            // Si hay Ready (aunque no tenga setter), úsalo
            var t = skill.GetType();
            var pr = t.GetProperty("Ready");
            if (pr != null && pr.PropertyType == typeof(bool))
                return (bool)pr.GetValue(skill);

            // Si no, dedúcelo por remaining
            return GetRemaining(skill) <= 0;
        }

        // === Escritura: NUNCA tocamos Ready si no tiene setter ===
        public static void SetRemaining(object skill, int value)
        {
            if (skill == null) return;
            var t = skill.GetType();
            var p = t.GetProperty("CooldownTurnsRemaining") ?? t.GetProperty("cooldownRemaining");
            if (p != null && p.PropertyType == typeof(int) && p.CanWrite) { p.SetValue(skill, value); return; }
            var f = t.GetField("CooldownTurnsRemaining") ?? t.GetField("cooldownRemaining");
            if (f != null && f.FieldType == typeof(int)) { f.SetValue(skill, value); return; }

            // Sin campos → usa cache interno
            _remainingMap[skill] = value;
        }

        public static void StartCooldown(object skill)
        {
            if (skill == null) return;
            int max = GetMax(skill);
            if (max <= 0) { SetRemaining(skill, 0); return; }
            SetRemaining(skill, max);
        }

        public static void ReduceCooldown(object skill, int turns)
        {
            if (skill == null || turns <= 0) return;
            int rem = GetRemaining(skill);
            int newVal = Math.Max(0, rem - turns);
            SetRemaining(skill, newVal);
        }

        // Comparador por referencia para usar objetos como clave de forma estable
        private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
        {
            public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();
            public new bool Equals(object x, object y) => ReferenceEquals(x, y);
            public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
        }
    }
}
