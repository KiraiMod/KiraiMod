using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using KiraiMod.Core;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace KiraiMod.Voice
{
    [BepInPlugin(GUID, "KM.Voice", "0.0.0")]
    [BepInDependency("me.kiraihooks.KiraiMod", BepInDependency.DependencyFlags.HardDependency)]
    [BepInDependency("me.kiraihooks.KiraiMod.Core", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BasePlugin
    {
        private const string GUID = "me.kiraihooks.KiraiMod.Voice";

        internal static ConfigFile cfg;
        internal static Harmony harmony;

        private AssetBundle bundle;

        public override void Load()
        {
            cfg = Config;
            harmony = new(GUID);

            Managers.ModuleManager.Register();
            Core.UI.LegacyGUIManager.OnLoad += GUIManager_OnLoad;
        }

        private void GUIManager_OnLoad()
        {
            MemoryStream mem = new();
            Assembly.GetExecutingAssembly().GetManifestResourceStream("KiraiMod.Voice.Lib.KiraiMod.Voice.GUI.AssetBundle").CopyTo(mem);
            bundle = AssetBundle.LoadFromMemory(mem.ToArray());
            bundle.hideFlags |= HideFlags.HideAndDontSave;

            Transform GUI = bundle.LoadAsset("assets/voice.gui.prefab")
                .Cast<GameObject>()
                .Instantiate()
                .transform;

            for (int i = 0; i < GUI.childCount; i++)
                GUI.GetChild(i).SetParent(Core.UI.LegacyGUIManager.UserInterface.transform);

            GUI.Destroy();
        }
    }
}
