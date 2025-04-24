using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using System.IO;
using REPOLib.Modules;

namespace GamblingMachine
{
    [BepInPlugin("Lluciocc.GamblingMachine", "GamblingMachine", "1.0")]
    [BepInDependency(REPOLib.MyPluginInfo.PLUGIN_GUID, BepInDependency.DependencyFlags.HardDependency)]

    public class GamblingMachine : BaseUnityPlugin
    {
        internal static GamblingMachine Instance { get; private set; } = null!;
        internal new static ManualLogSource Logger => Instance._logger;
        private ManualLogSource _logger => base.Logger;

        internal Harmony? Harmony { get; set; }
        public static GameObject SlotMachinePrefab { get; private set; } = null!;

        private void Awake()
        {
            Instance = this;

            this.gameObject.transform.parent = null;
            this.gameObject.hideFlags = HideFlags.HideAndDontSave;

            LoadAssetBundle();
            Patch();

            Logger.LogInfo($"{Info.Metadata.GUID} v{Info.Metadata.Version} has loaded!");
        }

        private void LoadAssetBundle() // basically its work
        {
            string bundlePath = Path.Combine(Paths.PluginPath, "Lluciocc-GamblingMachine", "slotbundle.machine");
            AssetBundle bundle = AssetBundle.LoadFromFile(bundlePath);

            if (bundle == null)
            {
                Logger.LogError("Cannot load AssetBundle !");
                return;
            }

            SlotMachinePrefab = bundle.LoadAsset<GameObject>("machine");

            if (SlotMachinePrefab == null)
            {
                Logger.LogError("Prefab 'machine' is unknow in the bundle !");
                return;
            }

            NetworkPrefabs.RegisterNetworkPrefab(SlotMachinePrefab);
            Logger.LogInfo("Prefab 'machine' has been loaded and registered with sucess");
        }

        private void Patch()
        {
            Harmony ??= new Harmony(Info.Metadata.GUID);
            Harmony.PatchAll();
        }

        internal void Unpatch()
        {
            Harmony?.UnpatchSelf();
        }
    }
}
