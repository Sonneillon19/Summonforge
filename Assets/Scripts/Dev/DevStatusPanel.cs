using UnityEngine;
using RPG.Battle;
using RPG.Combat;

public class DevStatusPanel : MonoBehaviour
{
    public TurnManager manager;
    public StatusEffectSO burn;
    public StatusEffectSO stun;

    UnitRuntime CurrentActor()
    {
        foreach (var u in manager.Units)
            if (!u.IsDead && u.Atb >= 1f) return u;
        return null;
    }

    UnitRuntime FirstEnemyFor(UnitRuntime u)
    {
        if (u == null) return null;
        foreach (var x in manager.Units)
            if (x.Team != u.Team && !x.IsDead) return x;
        return null;
    }

    // Botón: aplicar Burn al primer enemigo del actor
    public void Btn_ApplyBurnToEnemy()
    {
        var actor  = CurrentActor();
        var target = FirstEnemyFor(actor);
        if (actor?.Combat && target?.Combat && burn)
        {
            target.Combat.Status.Apply(burn, 2, 1, actor.Combat);
            Debug.Log($"[DEV] Burn → {target.Combat.displayName}");
        }
    }

    // Botón: aplicar Stun al actor (para probar control)
    public void Btn_ApplyStunToActor()
    {
        var actor = CurrentActor();
        if (actor?.Combat && stun)
        {
            actor.Combat.Status.Apply(stun, 1, 1, actor.Combat);
            Debug.Log($"[DEV] Stun → {actor.Combat.displayName}");
        }
    }

    // Botón: terminar turno
    public void Btn_EndTurn()
    {
        manager.EndTurn();
    }
}
