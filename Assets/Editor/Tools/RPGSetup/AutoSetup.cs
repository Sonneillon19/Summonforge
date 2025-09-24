#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using System.IO;
using RPG.Battle;

namespace RPG.EditorTools
{
    public static class AutoSetup
    {
        private const string ScenesPath = "Assets/Scenes";
        private const string SoPath = "Assets/ScriptableObjects";

        [MenuItem("Tools/RPG Setup/Create Sample Battle")]
        public static void CreateSampleBattle()
        {
            EnsureFolder("Assets", "Scenes");
            EnsureFolder("Assets", "ScriptableObjects");
            EnsureFolder(SoPath, "Units");
            EnsureFolder(SoPath, "Skills");
            EnsureFolder(SoPath, "Effects");

            // Skill
            var basicSkill = ScriptableObject.CreateInstance<SkillDef>();
            basicSkill.skillId = "skill_basic_strike";
            basicSkill.displayName = "Golpe Básico";
            basicSkill.cdBase = 2;
            basicSkill.atkScale = 1.0f;
            var basicSkillPath = $"{SoPath}/Skills/skill_basic_strike.asset";
            AssetDatabase.CreateAsset(basicSkill, basicSkillPath);

            // Effect (placeholder)
            var atkUp = ScriptableObject.CreateInstance<EffectDef>();
            atkUp.effectId = "buff_atk_up";
            atkUp.displayName = "ATK↑";
            atkUp.isBuff = true;
            atkUp.maxStacks = 1;
            var atkUpPath = $"{SoPath}/Effects/buff_atk_up.asset";
            AssetDatabase.CreateAsset(atkUp, atkUpPath);

            // Units
            var unitA = ScriptableObject.CreateInstance<UnitDef>();
            unitA.unitId = "unit_guardian";
            unitA.displayName = "Guardián";
            unitA.rarity = 3;
            unitA.role = "Tank";
            unitA.baseStats = new Stats { HP = 3000, ATK = 120, DEF = 150, SPD = 100, CritRate = 0.15f, CritDmg = 0.5f, Res = 0.15f, Acc = 0.0f };
            unitA.skills = new SkillDef[] { basicSkill };
            var unitAPath = $"{SoPath}/Units/unit_guardian.asset";
            AssetDatabase.CreateAsset(unitA, unitAPath);

            var unitB = ScriptableObject.CreateInstance<UnitDef>();
            unitB.unitId = "unit_lanzasombras";
            unitB.displayName = "LanzaSombras";
            unitB.rarity = 3;
            unitB.role = "DPS";
            unitB.baseStats = new Stats { HP = 2200, ATK = 180, DEF = 90, SPD = 115, CritRate = 0.20f, CritDmg = 0.7f, Res = 0.05f, Acc = 0.05f };
            unitB.skills = new SkillDef[] { basicSkill };
            var unitBPath = $"{SoPath}/Units/unit_lanzasombras.asset";
            AssetDatabase.CreateAsset(unitB, unitBPath);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Escena + GameObjects
            var scene = EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects, NewSceneMode.Single);

            var tmGO = new GameObject("TurnManager");
            var tm = tmGO.AddComponent<TurnManager>();

            var bootGO = new GameObject("SampleBattleBootstrap");
            var boot = bootGO.AddComponent<SampleBattleBootstrap>();
            boot.turnManager = tm;
            boot.unitA = unitA;
            boot.unitB = unitB;
            boot.basicSkill = basicSkill;

            if (!Directory.Exists(ScenesPath)) Directory.CreateDirectory(ScenesPath);
            var savedPath = $"{ScenesPath}/SampleBattle.unity";
            EditorSceneManager.SaveScene(scene, savedPath);
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "RPG Setup",
                "¡Listo! Se generó la escena 'SampleBattle' y SOs de ejemplo." +
                "Abre Assets/Scenes/SampleBattle.unity y presiona Play para ver logs en la consola.",
                "OK"
            );
        }

        private static void EnsureFolder(string parent, string child)
        {
            var path = System.IO.Path.Combine(parent, child).Replace("\\", "/");
            if (!AssetDatabase.IsValidFolder(path))
            {
                AssetDatabase.CreateFolder(parent, child);
            }
        }
    }
}
#endif
