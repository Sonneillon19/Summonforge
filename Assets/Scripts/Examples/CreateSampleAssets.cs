#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;
using RPG.Combat;
using RPG.Skills;

public static class CreateSampleAssets
{
    [MenuItem("RPG/Create Sample Status & Skills")]
    public static void CreateSamples()
    {
        // Asegurar carpetas
        EnsureFolder("Assets/Samples");
        EnsureFolder("Assets/Samples/Status");
        EnsureFolder("Assets/Samples/Skills");

        // ---- Status: Burn
        var burn = ScriptableObject.CreateInstance<StatusEffectSO>();
        burn.effectId = "burn"; burn.displayName = "Quemadura";
        burn.dotPerTick = 50; burn.type = StatusType.Dot;
        burn.uiPriority = 5; burn.isHarmful = true;
        CreateOrReplaceAsset(burn, "Assets/Samples/Status/burn.asset");

        // ---- Status: Stun
        var stun = ScriptableObject.CreateInstance<StatusEffectSO>();
        stun.effectId = "stun"; stun.displayName = "Aturdido";
        stun.preventsAction = true; stun.type = StatusType.Control;
        stun.uiPriority = 10; stun.isHarmful = true;
        CreateOrReplaceAsset(stun, "Assets/Samples/Status/stun.asset");

        // ---- Skill: Fireball
        var fireball = ScriptableObject.CreateInstance<ActiveSkill>();
        fireball.skillId = "fireball"; fireball.displayName = "Bola de Fuego";
        fireball.tags = SkillTag.Damage | SkillTag.ApplyStatus | SkillTag.SingleTarget;
        fireball.power = 2.0f; fireball.flatDamage = 0f;
        fireball.effectsToApply = new[] { burn };
        fireball.applyChance = 80; fireball.durationTurns = 2; fireball.stacksToAdd = 1;
        CreateOrReplaceAsset(fireball, "Assets/Samples/Skills/fireball.asset");

        // ---- Skill: Bash
        var bash = ScriptableObject.CreateInstance<ActiveSkill>();
        bash.skillId = "bash"; bash.displayName = "Golpe Aturdidor";
        bash.tags = SkillTag.Damage | SkillTag.ApplyStatus | SkillTag.SingleTarget;
        bash.power = 1.2f; bash.effectsToApply = new[] { stun };
        bash.applyChance = 60; bash.durationTurns = 1; bash.stacksToAdd = 1;
        CreateOrReplaceAsset(bash, "Assets/Samples/Skills/bash.asset");

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("RPG Samples", "Se crearon/actualizaron los assets en Assets/Samples.", "OK");
    }

    private static void EnsureFolder(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            AssetDatabase.ImportAsset(path);
        }
    }

    private static void CreateOrReplaceAsset(Object obj, string path)
    {
        var existing = AssetDatabase.LoadAssetAtPath<Object>(path);
        if (existing == null)
        {
            AssetDatabase.CreateAsset(obj, path);
        }
        else
        {
            // Reemplazar contenido manteniendo la misma ruta
            var old = existing;
            EditorUtility.CopySerialized(obj, old);
            Object.DestroyImmediate(obj, true);
        }
    }
}
#endif
