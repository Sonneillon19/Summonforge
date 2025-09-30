using UnityEngine;
using System.Collections.Generic;
using RPG.Battle;
using RPG.Combat;

public class DevStatusPanel : MonoBehaviour
{
    public TurnManager manager;
    public StatusEffectSO burn;
    public StatusEffectSO stun;

    UnitRuntime CurrentActorATB()
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

    public void Btn_ApplyBurnToEnemy()
    {
        var actor = CurrentActorATB();
        var target = FirstEnemyFor(actor);
        if (actor?.Combat && target?.Combat && burn)
        {
            target.Combat.Status.Apply(burn, 2, 1, actor.Combat);
            Debug.Log($"[DEV] Burn → {target.Combat.displayName}");
        }
    }

    public void Btn_ApplyStunToActor()
    {
        var actor = CurrentActorATB();
        if (actor?.Combat && stun)
        {
            actor.Combat.Status.Apply(stun, 1, 1, actor.Combat);
            Debug.Log($"[DEV] Stun → {actor.Combat.displayName}");
        }
    }

    // ✅ Botón: terminar turno
    public void Btn_EndTurn()
    {
        if (manager.flow == BattleFlow.ManualTurns)
        {
            // En el loop manual esto resolverá el turno actual (acción “pasar”)
            manager.SubmitPlayerAction(-1, null);
            return;
        }

        // ATB: termina el turno del actor activo
        var actor = CurrentActorATB();
        if (actor == null) return;

        var cu = actor.Combat;
        cu?.Status?.Tick(ApplyTiming.PerTurnEnd);
        actor.EndTurn();
        actor.ResetAtb();
        Debug.Log("[DEV] EndTurn (ATB).");
    }
}
