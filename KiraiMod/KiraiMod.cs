global using KiraiMod.Core;

using BepInEx;
using BepInEx.IL2CPP;
using KiraiMod.Managers;

namespace KiraiMod
{
    [BepInPlugin("me.kiraihooks.KiraiMod", "KiraiMod", "2.0.0.0")]
    [BepInDependency("me.kiraihooks.KiraiMod.Core")]
    [BepInDependency("me.kiraihooks.KiraiMod.Core.UI")]
    public class KiraiMod : BasePlugin
    {
        public override void Load()
        {
            Shared.Logger = Log;
            Shared.Config = Config;
            Shared.Harmony = new("me.kiraihooks.KiraiMod");

            typeof(ModuleManager).Initialize();
            typeof(GUIManager).Initialize();

            Events.PlayerJoined += player => Log.LogMessage($"{player.APIUser.displayName} joined");
            Events.PlayerLeft += player => Log.LogMessage($"{player.APIUser.displayName} left");
        }
    }
}
