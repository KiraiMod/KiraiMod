namespace KiraiMod.GUI
{
    public static class GUIManager
    {
        private static UnityEngine.AssetBundle bundle;

        static GUIManager()
        {
            InjectMonos();
            Events.UIManagerLoaded += OnUIManagerLoaded;
        }

        private static void InjectMonos()
        {
            UnhollowerRuntimeLib.ClassInjector.RegisterTypeInIl2Cpp(typeof(Behaviours.Dragable), false);
        }

        private static void OnUIManagerLoaded()
        {
            System.IO.MemoryStream mem = new();
            System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream("KiraiMod.Lib.KiraiMod.GUI.AssetBundle").CopyTo(mem);
            bundle = UnityEngine.AssetBundle.LoadFromMemory(mem.ToArray());
            bundle.hideFlags |= UnityEngine.HideFlags.HideAndDontSave;

            bundle.LoadAsset("assets/kiraimod.gui.prefab")
                .Cast<UnityEngine.GameObject>()
                .Instantiate()
                .DontDestroyOnLoad()
                .name = "KiraiMod.GUI";

            Shared.Logger.LogInfo("Loaded GUI");
        }
    }
}
