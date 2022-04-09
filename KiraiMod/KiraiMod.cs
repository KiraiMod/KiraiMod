global using KiraiMod.Core;

using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using HarmonyLib;
using KiraiMod.Managers;

namespace KiraiMod
{
    [BepInPlugin("me.kiraihooks.KiraiMod", "KiraiMod", "2.0.0.0")]
    [BepInDependency("me.kiraihooks.KiraiMod.Core")]
    [BepInDependency("me.kiraihooks.KiraiMod.Core.UI")]
    public class Plugin : BasePlugin
    {
        internal static ManualLogSource log;
        internal static ConfigFile cfg;
        internal static Harmony harmony;

        public override void Load()
        {
            log = Log;
            cfg = Config;
            harmony = new("me.kiraihooks.KiraiMod");

            typeof(ModuleManager).Initialize();
            typeof(GUIManager).Initialize();
        }
    }
}
