using BepInEx.Configuration;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace KiraiMod.Managers
{
    public static class GUIManager
    {
        public static GameObject GUI;
        public static Transform UserInterface;

        private static AssetBundle bundle;

        static GUIManager()
        {
            Events.UIManagerLoaded += OnUIManagerLoaded;

            Shared.Config.Bind(
                "GUI",
                "Keybind",
                new Key[] { Key.RightShift },
                "The keybind you want to use to open the GUI"
            ).RegisterKeybind(() => UserInterface.gameObject.active ^= true);
        }

        private static void OnUIManagerLoaded()
        {
            MemoryStream mem = new();
            Assembly.GetExecutingAssembly().GetManifestResourceStream("KiraiMod.Lib.KiraiMod.GUI.AssetBundle").CopyTo(mem);
            bundle = AssetBundle.LoadFromMemory(mem.ToArray());
            bundle.hideFlags |= HideFlags.HideAndDontSave;

            Load();
            Setup();
        }

        private static void Load()
        {
            GUI = bundle.LoadAsset("assets/kiraimod.gui.prefab")
                .Cast<GameObject>()
                .Instantiate()
                .DontDestroyOnLoad();

            GUI.name = "KiraiMod.GUI";

            Shared.Logger.LogInfo("Loaded GUI");
        }

        private static void Setup()
        {
            Stopwatch sw = new();

            UserInterface = GUI.transform.Find("UserInterface");

            Type[] handlers = ModuleManager.Modules["KiraiMod.GUI"];

            for (int i = 0; i < UserInterface.childCount; i++)
            {
                Transform child = UserInterface.GetChild(i);
                Type handler = handlers.FirstOrDefault(x => x.Name == child.name);

                if (handler == null)
                {
                    Shared.Logger.LogWarning($"Failed to find GUI handler for {child.name}");
                    continue;
                }

                MethodInfo setup = handler.GetMethod("Setup");
                if (setup == null)
                {
                    Shared.Logger.LogWarning($"Missing setup method for GUI handler {child.name}");
                    continue;
                }

                setup.Invoke(null, new object[1] { child });
            }

            UserInterface.gameObject.active = false;

            sw.Stop();

            Shared.Logger.LogInfo($"Setup GUI in {sw.Elapsed.Milliseconds} ms");
        }

        public static void GUIBind(this ConfigEntry<bool> entry, Toggle toggle) => entry.SettingChanged += ((EventHandler)((sender, args) => toggle.Set(entry.Value, false))).Invoke();
        public static void GUIBind(this ConfigEntry<float> entry, KiraiSlider toggle) => entry.SettingChanged += ((EventHandler)((sender, args) => toggle.slider.Set(entry.Value))).Invoke();

        public class KiraiSlider
        {
            public Text text;
            public Slider slider;

            public KiraiSlider(Transform obj)
            {
                text = obj.GetComponent<Text>();
                slider = obj.GetComponent<Slider>();

                if (text != null)
                {
                    string format = text.text;
                    slider.onValueChanged.AddListener(new Action<float>(value => text.text = string.Format(format, value)));
                }
            }
        }
    }
}
