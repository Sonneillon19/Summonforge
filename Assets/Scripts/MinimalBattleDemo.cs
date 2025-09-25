using UnityEngine;
using RPG.Combat;
using RPG.Skills;

#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem; // New Input System
#endif

public class MinimalBattleDemo : MonoBehaviour
{
    [Header("Refs")]
    public CombatUnit caster;
    public CombatUnit target;

    [Header("Skills (ScriptableObjects)")]
    public Skill fireball;
    public Skill bash;

    private bool casterTurn = true;

    void Start()
    {
        caster.Stats.MaxHP = caster.Stats.HP = 1200;
        target.Stats.MaxHP = target.Stats.HP = 1500;
    }

    void Update()
    {
#if ENABLE_INPUT_SYSTEM
        var kb = Keyboard.current;
        if (kb != null)
        {
            if (kb.digit1Key.wasPressedThisFrame) CastFireball();
            if (kb.digit2Key.wasPressedThisFrame) CastBash();
            if (kb.spaceKey.wasPressedThisFrame)  EndTurn();
        }
#else
        if (Input.GetKeyDown(KeyCode.Alpha1)) CastFireball();
        if (Input.GetKeyDown(KeyCode.Alpha2)) CastBash();
        if (Input.GetKeyDown(KeyCode.Space))  EndTurn();
#endif
    }

    public void CastFireball()
    {
        if (fireball == null || caster == null || target == null) return;
        caster.UseSkill(fireball, new[] { target });
        Debug.Log($"Fireball → {target.displayName} | HP: {target.Stats.HP}/{target.Stats.MaxHP}");
    }

    public void CastBash()
    {
        if (bash == null || caster == null || target == null) return;
        caster.UseSkill(bash, new[] { target });
        Debug.Log($"Bash → {target.displayName} | HP: {target.Stats.HP}/{target.Stats.MaxHP}");
    }

    public void EndTurn()
    {
        if (casterTurn)
        {
            caster.Status.Tick(ApplyTiming.PerTurnEnd);
            target.Status.Tick(ApplyTiming.PerTurnStart);
        }
        else
        {
            target.Status.Tick(ApplyTiming.PerTurnEnd);
            caster.Status.Tick(ApplyTiming.PerTurnStart);
        }
        casterTurn = !casterTurn;
        Debug.Log($"EndTurn → Ahora turno de {(casterTurn ? "Caster" : "Target")}");
    }
}
