using UnityEngine;
using RPG.Combat;


namespace RPG.Skills
{
[CreateAssetMenu(menuName = "RPG/Skills/ActiveSkill")]
public class ActiveSkill : Skill
{
[Header("Numeros básicos")]
public float power = 1.0f; // multiplicador de ATK del usuario para daño base
public float flatDamage = 0f; // daño plano adicional
public float healRatio = 0f; // multiplicador de curación sobre ATK o HP


[Header("Estados a aplicar (opcional)")]
public StatusEffectSO[] effectsToApply;
[Range(0,100)] public int applyChance = 100;
public int durationTurns = 2;
public int stacksToAdd = 1;


public override void Execute(CombatUnit user, CombatUnit[] targets)
{
if (targets == null || targets.Length == 0) return;


foreach (var t in targets)
{
if (t == null || !t.IsAlive) continue;


// Daño
if (tags.HasFlag(SkillTag.Damage))
{
float atk = user.Stats.Attack;
float damage = atk * power + flatDamage;
t.TakeDamage(Mathf.RoundToInt(damage), user, this);
}


// Curación
if (tags.HasFlag(SkillTag.Heal))
{
float baseVal = user.Stats.Attack; // o user.Stats.MaxHP si quieres curación por vida
int heal = Mathf.RoundToInt(baseVal * healRatio);
t.Heal(heal, user, this);
}


// Aplicación de estados
if (effectsToApply != null && effectsToApply.Length > 0 && Random.Range(0,100) < applyChance)
{
foreach (var so in effectsToApply)
{
if (so == null) continue;
t.Status.Apply(so, durationTurns, stacksToAdd, user);
}
}
}
}
}
}