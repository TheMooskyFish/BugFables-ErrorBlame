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
        private ConfigEntry<bool> _bfPlusSupport;
        internal static AssetBundle Bundle;

        public void Awake()
        {
            Logger = base.Logger;
            _bfPlusSupport = Config.Bind("Config", "Bypass Bug Fables Plus Check", false);
            if (CheckBlacklist()) return;

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

            //new GameObject("debug").AddComponent<Debug>();
        }

        //internal sealed class Debug : MonoBehaviour
        //{
        //    public void Start()
        //    {
        //        DontDestroyOnLoad(this);
        //    }
        //    public void Update()
        //    {
        //        if (Input.GetKeyDown(KeyCode.Y))
        //        {
        //            Bundle.Unload(true);
        //            Bundle = AssetBundle.LoadFromFile($"{Paths.PluginPath}/ErrorBlame/errorblame");
        //            SceneManager.LoadScene(0);
        //            Logger.LogInfo("Reloaded");
        //        }
        //        
        //        if (Input.GetKeyDown(KeyCode.G))
        //        {
        //            TextAsset[] collection = Resources.LoadAll<TextAsset>("");
        //            Logger.LogInfo("------------------");
        //            foreach (TextAsset text in Bundle.LoadAllAssets<TextAsset>())
        //            {
        //                TextAsset og = collection.FirstOrDefault(i => i.name == text.name)!;
        //                if (og.text.Split('\n').Length != text.text.Split('\n').Length)
        //                {
        //                    Logger.LogInfo(
        //                        $"{text.name} - OG:{og.text.Split('\n').Length} - EB:{text.text.Split('\n').Length}");
        //                }
        //            }
        //            Logger.LogInfo("------------------");
        //        }
        //    }
        //}


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