using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using RPG.Battle;

public class ManualTurnUI : MonoBehaviour
{
    [Header("Refs")]
    public TurnManager manager;
    public BattleInput input;      // opcional (para usar el target seleccionado)
    public GameObject panel;       // panel que se muestra/oculta en el turno del jugador

    [Header("Botones de skills")]
    public Button[]   skillButtons;   // asigna 3–4
    public TMP_Text[] skillLabels;    // mismos índices que los botones

    private UnitRuntime _actor;

    // ---------- Lifecycle / Eventos ----------
    void OnEnable()
    {
        if (manager != null)
        {
            // evita dobles suscripciones si el GO se re-activa
            manager.OnPlayerTurnBegan -= HandlePlayerTurn;
            manager.OnPlayerTurnBegan += HandlePlayerTurn;
        }
        if (panel) panel.SetActive(false);
        Debug.Log("[UI] ManualTurnUI activo y suscrito.");
    }

    void OnDisable()
    {
        if (manager != null)
            manager.OnPlayerTurnBegan -= HandlePlayerTurn;
    }

    // ---------- Helpers ----------
    private string SafeName(UnitRuntime u)
    {
        try
        {
            var cu = u?.Combat;
            if (cu != null && !string.IsNullOrEmpty(cu.displayName)) return cu.displayName;
            return u != null ? u.ToString() : "(null)";
        }
        catch { return "(ex)"; }
    }

    private string ResolveSkillName(object skill, int fallbackIndex)
    {
        var t = skill.GetType();
        var nameProp = t.GetProperty("DisplayName") ?? t.GetProperty("Name");
        if (nameProp != null)
        {
            var val = nameProp.GetValue(skill) as string;
            if (!string.IsNullOrEmpty(val)) return val;
        }
        return $"Skill {fallbackIndex + 1}";
    }

    // ---------- UI principal ----------
    private void HandlePlayerTurn(UnitRuntime actor)
    {
        if (actor == null)
        {
            Debug.LogWarning("[UI] HandlePlayerTurn llamado con actor NULL");
            return;
        }
        if (panel == null)
        {
            Debug.LogWarning("[UI] Panel no asignado en ManualTurnUI");
            return;
        }

        _actor = actor;
        Debug.Log($"[UI] Turno de jugador: {SafeName(actor)} (Combat={(actor.Combat ? "OK" : "NULL")})");

        panel.SetActive(true);

        // pintar/armar botones
        int btnCount = skillButtons != null ? skillButtons.Length : 0;
        for (int i = 0; i < btnCount; i++)
        {
            var btn = skillButtons[i];
            var lbl = (skillLabels != null && i < skillLabels.Length) ? skillLabels[i] : null;

            bool hasSkill = (actor.Skills != null && i < actor.Skills.Count && actor.Skills[i] != null);
            if (!hasSkill)
            {
                if (btn) btn.gameObject.SetActive(false);
                if (lbl) lbl.text = "";
                continue;
            }

            var s = actor.Skills[i];
            string baseName = ResolveSkillName(s, i);

            // Skill 1 (idx 0) SIEMPRE activa
            bool ready = (i == 0) ? true : SkillCooldownAdapter.GetReady(s);
            int  rem   = (i == 0) ? 0    : SkillCooldownAdapter.GetRemaining(s);

            if (lbl) lbl.text = ready ? baseName : $"{baseName} (CD:{rem})";
            if (btn)
            {
                btn.interactable = ready;
                btn.onClick.RemoveAllListeners();
                int idx = i; // capturar índice
                btn.onClick.AddListener(() => OnClickSkill(idx));
                btn.gameObject.SetActive(true);
            }
        }
    }
    public void OnClickSkill(int skillIndex)
    {
        if (manager == null) return;

        var targets = new List<UnitRuntime>();
        if (input != null && input.IsValidEnemyTarget(input.SelectedTarget, Team.Player))
            targets.Add(input.SelectedTarget);

        manager.SubmitPlayerAction(skillIndex, targets);

        if (panel) panel.SetActive(false);
    }
}
