using UnityEngine;
using RPG.Skills;

namespace RPG.Combat
{
    public class CombatUnit : MonoBehaviour
    {
        public string unitId;
        public string displayName;
        public StatsBlock Stats = new StatsBlock();
        public StatusController Status { get; private set; }
        public bool IsAlive => Stats.HP > 0;

        private void Awake()
        {
            Status = GetComponent<StatusController>();
            if (Status == null) Status = gameObject.AddComponent<StatusController>();
            Status.BindOwner(this);
        }

        public void TakeDamage(int amount, CombatUnit source, Skill fromSkill)
        {
            int final = Mathf.Max(0, amount - Mathf.RoundToInt(Stats.Defense * 0.2f));
            Stats.HP = Mathf.Max(0, Stats.HP - final);
            Status?.RaiseOnDamaged(final, source, fromSkill);
            if (Stats.HP == 0) Status?.RaiseOnDeath(source);
        }

        public void Heal(int amount, CombatUnit source, Skill fromSkill)
        {
            Stats.HP = Mathf.Min(Stats.MaxHP, Stats.HP + amount);
            Status?.RaiseOnHealed(amount, source, fromSkill);
        }

        public void UseSkill(Skill skill, CombatUnit[] targets)
        {
            if (!IsAlive || skill == null) return;
            skill.Execute(this, targets);
        }
    }
}
