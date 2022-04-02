using BepInEx;
using BepInEx.Configuration;
using BepInEx.IL2CPP;
using HarmonyLib;
using KiraiMod.Core;

namespace KiraiMod.Voice
{
    [BepInPlugin(GUID, "KM.Voice", "0.0.0")]
    [BepInDependency("me.kiraihooks.KiraiMod.Core", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BasePlugin
    {
        private const string GUID = "me.kiraihooks.KiraiMod.Voice";

        internal static ConfigFile cfg;
        internal static Harmony harmony;

        public override void Load()
        {
            cfg = Config;
            harmony = new(GUID);

            typeof(Voice).Initialize();
        }
    }
}
