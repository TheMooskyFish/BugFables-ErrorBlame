using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

namespace ErrorBlame
{

    [HarmonyPatch(typeof(Resources))]
    internal sealed class ResourcesPatch
    {
        [HarmonyPatch("Load", typeof(string), typeof(Type))]
        [HarmonyPostfix]
        private static void Load(string path, Type systemTypeInstance, ref object __result)
        {
            if (__result is not TextAsset text) return;
            TextAsset replacement = ErrorBlameCore.GetFile(path.Split('/').Last());
            if (!replacement) return;
            ErrorBlameCore.Logger.LogInfo(
                $"{text.name} - {text.text.Split('\n').Length} - {replacement.text.Split('\n').Length}");
            __result = replacement;
        }

        //could be used for full bug fables plus support
        //[HarmonyPatch(typeof(AssetBundle), "LoadAllAssets", typeof(Type)) ]
        //[HarmonyPostfix]
        //private static void LoadAllAssets(ref object __result)
        //{
        //    return;
        //}
        
        //[HarmonyPatch(typeof(MainManager), "Start")]
        //internal sealed class StartMainPatch
        //{
        //    private static void Postfix()
        //    {
        //        if (!GameObject.Find("debug"))
        //        {
        //            new GameObject("debug").AddComponent<ErrorBlameCore.debug>();
        //        }
        //    }
        //}

        [HarmonyPatch("LoadAll", typeof(string), typeof(Type))]
        [HarmonyPostfix]
        private static void LoadAll(string path, Type systemTypeInstance, ref object __result)
        {
            if (path.Contains("GUI/title"))
            {
                __result = new[] { ErrorBlameCore.Bundle.LoadAsset<Sprite>("title0") };
            }

            if (path.Contains("GUI/BattleMessage/rank"))
            {
                __result = ErrorBlameCore.Bundle.LoadAssetWithSubAssets<Sprite>("rank0");
            }

            if (path.Contains("BattleMessage/battlem0"))
            {
                __result = ErrorBlameCore.Bundle.LoadAssetWithSubAssets<Sprite>("battlem0");
            }
        }
    }
    [HarmonyPatch(typeof(MainManager))]
    internal sealed class MainManagerPatch
    {
        [HarmonyPatch("OrganizeLines"), HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> RemoveEnglishCheck(IEnumerable<CodeInstruction> instructions)
        {
            return new CodeMatcher(instructions).MatchForward(
                true,
                new CodeMatch(OpCodes.Ldsfld, AccessTools.Field(typeof(MainManager), "languageid")),
                new CodeMatch(OpCodes.Ldc_I4_0),
                new CodeMatch(OpCodes.Ble)
            ).Advance(-1).SetOpcodeAndAdvance(OpCodes.Ldc_I4_M1).InstructionEnumeration();
        }
        [HarmonyPatch("LateUpdate"), HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> RemoveEnglishMessageBreak(IEnumerable<CodeInstruction> instructions)
        {
            CodeMatcher codeMatcher = new CodeMatcher(instructions).Start();
            foreach (CodeInstruction _ in codeMatcher.InstructionsWithOffsets(0, 6))
                codeMatcher.SetAndAdvance(OpCodes.Nop, null);
            return codeMatcher.InstructionEnumeration();
        }
    }
}