using System;

namespace KiraiMod.Managers
{
    public static class ManagerManager
    {
        static ManagerManager()
        {
            typeof(ModuleManager).Initialize();
            typeof(GUIManager).Initialize();
            typeof(KeybindManager).Initialize();
        }
    }
}
