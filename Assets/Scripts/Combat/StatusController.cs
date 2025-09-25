using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RPG.Skills; // para el tipo Skill en los eventos

namespace RPG.Combat
{
    [DisallowMultipleComponent]
    public class StatusController : MonoBehaviour
    {
        private CombatUnit owner;
        private readonly List<StatusEffectInstance> _effects = new();

        public event Action<StatusEffectInstance> OnEffectAdded;
        public event Action<StatusEffectInstance> OnEffectUpdated;
        public event Action<StatusEffectInstance> OnEffectRemoved;
        public event Action<int, CombatUnit, Skill> OnDamaged;
        public event Action<int, CombatUnit, Skill> OnHealed;
        public event Action<CombatUnit> OnDeath;

        public void BindOwner(CombatUnit cu) => owner = cu;

        public IReadOnlyList<StatusEffectInstance> Effects => _effects;

        public void Apply(StatusEffectSO so, int duration, int stacks, CombatUnit from)
        {
            Debug.Log($"[Status] GO={name} owner={(owner ? owner.displayName : "NULL")} recibe {so?.displayName} (icon={(so?.icon ? so.icon.name : "NULL")})");
            var existing = _effects.FirstOrDefault(e => e.data == so);
            if (existing != null)
            {
                existing.remainingTurns = Mathf.Max(existing.remainingTurns, duration);
                existing.stacks = Mathf.Min(so.maxStacks, existing.stacks + stacks);
                OnEffectUpdated?.Invoke(existing);
            }
            else
            {
                var inst = new StatusEffectInstance(so, duration, stacks, from);
                _effects.Add(inst);
                SortByUiPriority();
                OnEffectAdded?.Invoke(inst);
            }
        }

        public void Remove(StatusEffectSO so)
        {
            var inst = _effects.FirstOrDefault(e => e.data == so);
            if (inst != null)
            {
                _effects.Remove(inst);
                OnEffectRemoved?.Invoke(inst);
            }
        }

        public void Tick(ApplyTiming when)
        {
            // Efectos con tick
            for (int i = _effects.Count - 1; i >= 0; i--)
            {
                var e = _effects[i];
                if (e.data.tickTiming == when)
                {
                    if (e.data.dotPerTick != 0 && owner != null)
                    {
                        int damage = e.data.dotPerTick * e.stacks;
                        owner.TakeDamage(damage, e.source, null);
                    }
                }
            }

            // Reducir duración al final del turno
            if (when == ApplyTiming.PerTurnEnd)
            {
                for (int i = _effects.Count - 1; i >= 0; i--)
                {
                    _effects[i].remainingTurns--;
                    if (_effects[i].remainingTurns <= 0)
                    {
                        var removed = _effects[i];
                        _effects.RemoveAt(i);
                        OnEffectRemoved?.Invoke(removed);
                    }
                    else
                    {
                        OnEffectUpdated?.Invoke(_effects[i]);
                    }
                }
            }
        }

        public float GetAttackMultiplier()
        {
            float mod = 1f;
            foreach (var e in _effects)
                mod += e.data.atkModPercent * e.stacks;
            return Mathf.Max(0f, mod);
        }

        public float GetDefenseMultiplier()
        {
            float mod = 1f;
            foreach (var e in _effects)
                mod += e.data.defModPercent * e.stacks;
            return Mathf.Max(0f, mod);
        }

        public bool IsActionPrevented()
        {
            return _effects.Any(e => e.data.preventsAction);
        }

        // ==== Eventos “raise” que llama CombatUnit ====
        public void RaiseOnDamaged(int amount, CombatUnit src, Skill sk) => OnDamaged?.Invoke(amount, src, sk);
        public void RaiseOnHealed (int amount, CombatUnit src, Skill sk) => OnHealed?.Invoke(amount, src, sk);
        public void RaiseOnDeath  (CombatUnit killer)                 => OnDeath?.Invoke(killer);

        // ==== Orden visual por prioridad en HUD ====
        private void SortByUiPriority()
        {
            _effects.Sort((a, b) => b.data.uiPriority.CompareTo(a.data.uiPriority));
        }
    }
}
