namespace KiraiMod.Managers
{
    public static class ManagerManager
    {
        static ManagerManager()
        {
            typeof(KeybindManager).Initialize();
            typeof(ModuleManager).Initialize();
            typeof(GUIManager).Initialize();
        }
    }
}
