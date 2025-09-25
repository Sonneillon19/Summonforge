using RPG.Combat;
using UnityEngine;


namespace RPG.Skills
{
[CreateAssetMenu(menuName = "RPG/Skills/PassiveSkill")]
public class PassiveSkill : Skill
{
[TextArea]
public string passiveNotes;


public override void Execute(CombatUnit user, CombatUnit[] targets)
{
// Las pasivas se enganchan en eventos del CombatUnit/StatusController.
// Aquí no ejecutamos nada directamente.
}
}
}