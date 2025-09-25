using UnityEngine;
using RPG.Combat;


namespace RPG.Skills
{
public abstract class Skill : ScriptableObject
{
[Header("Meta")]
public string skillId;
public string displayName;
[TextArea] public string description;
public Sprite icon;
public SkillTag tags;
public Element element = Element.Neutral;
public int baseCooldown = 0; // en turnos
public SkillTargeting targeting = new SkillTargeting();


// Hook principal
public abstract void Execute(CombatUnit user, CombatUnit[] targets);


// Para tooltips o UI
public virtual string GetShortDesc() => description;
}
}