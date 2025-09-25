using RPG.Combat;


namespace RPG.Skills
{
[System.Serializable]
public class SkillTargeting
{
public TargetTeam targetTeam = TargetTeam.Enemy;
public int maxTargets = 1; // 1 para single-target, >1 o 0 para all
public bool includeSelf = false;
}
}