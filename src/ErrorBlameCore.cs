using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System;
using UnityEngine;

namespace ErrorBlame
{
    [BepInPlugin("com.bugfables.errorblame", "Error Blame: Eternal Fruit", "1.0.0")]
    [BepInProcess("Bug Fables.exe")]
    public sealed class ErrorBlameCore : BaseUnityPlugin
    {
        internal new static ManualLogSource Logger;
        internal static AssetBundle Bundle;
        private ConfigEntry<bool> _bfPlusSupport;

        public void Awake()
        {
            Logger = base.Logger;
            _bfPlusSupport = Config.Bind("Config", "Bypass Bug Fables Plus Check", false);
            if (CheckBlacklist())
                return;

            Bundle = AssetBundle.LoadFromFile($"{Paths.PluginPath}/ErrorBlame/errorblame");
            if (Bundle == null)
            {
                Logger.LogError("Missing AssetBundle!! - Stopping Loading");
                return;
            }

            Harmony harmony = new("com.bugfables.errorblame");
            try
            {
                harmony.PatchAll();
                Logger.LogInfo("Loaded");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex);
            }
        }


        private bool CheckBlacklist()
        {
            if (Application.version.StartsWith("1.1"))
            {
                Logger.LogError("Unsupported game version");
                return true;
            }

            if (!_bfPlusSupport.Value && Chainloader.PluginInfos.ContainsKey("com.Lyght.BugFables.plugins.BFPlus"))
            {
                Logger.LogError("Error Blame doesn't support Bug Fables Plus");
                return true;
            }

            return false;
        }

        public static TextAsset GetFile(string name)
        {
            return Bundle.Contains(name) ? Bundle.LoadAsset<TextAsset>(name) : null;
        }
    }
}