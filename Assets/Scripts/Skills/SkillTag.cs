namespace RPG.Skills
{
[System.Flags]
public enum SkillTag
{
None = 0,
SingleTarget = 1 << 0,
Area = 1 << 1,
Damage = 1 << 2,
Heal = 1 << 3,
ApplyStatus = 1 << 4,
Cleanse = 1 << 5,
IgnoreDef = 1 << 6,
TurnGain = 1 << 7,
}
}