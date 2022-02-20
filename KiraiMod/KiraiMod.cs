global using Object = UnityEngine.Object;

using BepInEx;
using BepInEx.IL2CPP;
using KiraiMod.Managers;

namespace KiraiMod
{
    [BepInPlugin("me.kiraihooks.KiraiMod", "KiraiMod", "2.0.0.0")]
    public class KiraiMod : BasePlugin
    {
        public override void Load()
        {
            Shared.Logger = Log;
            Shared.Config = Config;
            Shared.Harmony = new("me.kiraihooks.KiraiMod");

            typeof(Events).Initialize();

            typeof(KeybindManager).Initialize();
            typeof(ModuleManager).Initialize();
            typeof(GUIManager).Initialize(); 
        }
    }
}
