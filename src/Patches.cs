using HarmonyLib;
using System;
using System.Linq;
using UnityEngine;

namespace ErrorBlame
{
    [HarmonyPatch(typeof(Resources))]
    public class ResourcesPatch
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

    //[HarmonyPatch(typeof(CardGame), "StartCard")]
    //public class CardGamePatches
    //{
    //    [HarmonyPatch("StartCard")]
    //    [HarmonyPostfix]
    //    private static void StartCardPrefix(CardGame __instance)
    //    {
    //        __instance.carddiag = ErrorBlameCore.GetFile("CardDialogue").text.Split('\n');
    //    }
    //
    //    [HarmonyPatch("LoadCardData")]
    //    [HarmonyPostfix]
    //    private static void LoadCardDataPrefix(CardGame __instance)
    //    {
    //        string[] cards = ErrorBlameCore.GetFile("CardText").text.Split('\n');
    //        for (int i = 0; i < cards.Length; i++)
    //        {
    //            __instance.carddata[i].desc = cards[i].Split('@')[0];
    //        }
    //    }
    //}
}