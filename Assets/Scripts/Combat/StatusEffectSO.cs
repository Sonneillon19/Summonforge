using UnityEngine;


namespace RPG.Combat
{
[CreateAssetMenu(menuName = "RPG/Status/Effect")]
public class StatusEffectSO : ScriptableObject
{
[Header("Identidad")]
public string effectId;
public string displayName;
[TextArea] public string description;
public Sprite icon;
public StatusType type = StatusType.Other;
public int uiPriority = 0; // mayor = más prioridad para ordenar en HUD


[Header("Dinámica")]
public ApplyTiming tickTiming = ApplyTiming.PerTurnStart;
public int maxStacks = 10;
public bool isHarmful = false; // útil para cleanses


[Header("Parámetros comunes (opcional)")]
public float atkModPercent; // +0.2 = +20% ATK
public float defModPercent; // +0.2 = +20% DEF
public float speedModPercent; // etc.
public int dotPerTick; // daño por tick si es DoT
public bool preventsAction; // si es control (stun, freeze)
}
}