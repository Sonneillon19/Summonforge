using UnityEngine;


namespace RPG.Combat
{
public enum Element { Neutral, Fire, Water, Wind, Light, Dark }


public enum TargetTeam { Self, Ally, Enemy, AllAllies, AllEnemies }


public enum ApplyTiming { Instant, PerTurnStart, PerTurnEnd, PerSecond }


public enum StatusType { Buff, Debuff, Dot, Control, Shield, Other }
}