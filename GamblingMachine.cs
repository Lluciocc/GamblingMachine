using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using System.IO;
using REPOLib.Modules;
using System.Reflection;
using BepInEx.Configuration;
using REPOConfig;
using UnityEngine.Animations;



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

        // Config
        public static ConfigEntry<float> winrate;
        public static ConfigEntry<int> bet;
        public static ConfigEntry<float> winMultiplicator;
        public static ConfigEntry<bool> debug;

        private void Awake()
        {
            Instance = this;

            this.gameObject.transform.parent = null;
            this.gameObject.hideFlags = HideFlags.HideAndDontSave;

            bet = Config.Bind("Gameplay", "bet", 2, new ConfigDescription("Amount to bet", new AcceptableValueRange<int>(0, 100)));
            winMultiplicator = Config.Bind("Gameplay", "winMultiplicator", 2f, new ConfigDescription("Multiplicator of your bet when you win", new AcceptableValueRange<float>(1f, 5f)));
            winrate = Config.Bind("Gameplay", "winrate", 0.5f, new ConfigDescription("Chance to win (0.5 is 50%)", new AcceptableValueRange<float>(0f, 1f)));
            debug = Config.Bind("Debug", "EnableDebug", false, "Enable Debug Mode");

            LoadAssetBundle();
            Patch();

            Logger.LogInfo($"{Info.Metadata.GUID} v{Info.Metadata.Version} has loaded!");
        }

        private void LoadAssetBundle() // basically its work
        {
            string bundlePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "slotbundle.machine");
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
