using BepInEx;
using BepInEx.IL2CPP;

namespace KiraiMod
{
    [BepInPlugin("me.kiraihooks.KiraiMod", "KiraiMod", "2.0.0.0")]
    public class KiraiMod : BasePlugin
    {
        public override void Load()
        {
            Shared.Logger = Log;
        }

        public override bool Unload()
        {
            return base.Unload();
        }
    }
}
