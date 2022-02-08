namespace KiraiMod.Managers
{
    public static class AssetManager
    {
        private static UnityEngine.AssetBundle bundle;

        static AssetManager() {
            Events.ApplicationStart += OnApplicationStart;
        }

        private static void OnApplicationStart()
        {
            System.IO.MemoryStream mem = new();
            System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("KiraiMod.Lib.KiraiMod.GUI.AssetBundle").CopyTo(mem);
            bundle = UnityEngine.AssetBundle.LoadFromMemory(mem.ToArray());

            bundle.LoadAsset("assets/kiraimod.gui.prefab")
                .Cast<UnityEngine.GameObject>()
                .Instantiate()
                .DontDestroyOnLoad();

            Shared.Logger.LogInfo("Loaded GUI");
        }
    }
}
