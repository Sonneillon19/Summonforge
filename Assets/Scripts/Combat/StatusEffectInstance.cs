using System;


namespace RPG.Combat
{
[Serializable]
public class StatusEffectInstance
{
public StatusEffectSO data;
public int remainingTurns;
public int stacks;
public CombatUnit source;


public StatusEffectInstance(StatusEffectSO so, int duration, int addStacks, CombatUnit from)
{
data = so;
remainingTurns = Math.Max(1, duration);
stacks = Math.Max(1, addStacks);
source = from;
}
}
}