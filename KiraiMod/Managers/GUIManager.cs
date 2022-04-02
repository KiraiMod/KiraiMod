using BepInEx.Configuration;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.UI;

namespace KiraiMod.Managers
{
    public static class GUIManager
    {
        static GUIManager()
        {
            Core.UI.LegacyGUIManager.OnLoad += LoadGUI;
            Core.UI.LegacyGUIManager.OnLoadLate += SetupHandlers;

            if (Shared.Config.Bind("GUI", "SideUI", true, "Should the GUI manager automatically start up the SideUI adapter?").Value)
                Core.UI.SideUI.Adapter.Initialize();
        }

        private static void LoadGUI()
        {
            MemoryStream mem = new();
            Assembly.GetExecutingAssembly().GetManifestResourceStream("KiraiMod.Lib.KiraiMod.GUI.AssetBundle").CopyTo(mem);
            AssetBundle bundle = AssetBundle.LoadFromMemory(mem.ToArray());
            bundle.hideFlags |= HideFlags.HideAndDontSave;
            Transform GUI = bundle.LoadAsset("assets/gui.prefab")
                .Cast<GameObject>()
                .Instantiate()
                .transform;

            for (int i = 0; i < GUI.childCount; i++)
                GUI.GetChild(i).SetParent(Core.UI.LegacyGUIManager.UserInterface.transform);

            GUI.Destroy();
        }

        private static void SetupHandlers()
        {
            Stopwatch sw = new();
            sw.Start();

            Type[] handlers = ModuleManager.Modules["GUI"];

            Console.WriteLine("settingu p handlers: " + Core.UI.LegacyGUIManager.UserInterface.transform.childCount);
            for (int i = 0; i < Core.UI.LegacyGUIManager.UserInterface.transform.childCount; i++)
            {
                Transform child = Core.UI.LegacyGUIManager.UserInterface.transform.GetChild(i);
            Console.WriteLine(child.name);
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

                try
                {
                    setup.Invoke(null, new object[1] { child });
                }
                catch (Exception ex)
                {
                    Shared.Logger.LogError($"Setup method for GUI handler {child.name} failed: {ex}");
                    child.gameObject.active = false;
                }
            }

            Core.UI.LegacyGUIManager.UserInterface.active = false;

            sw.Stop();

            Shared.Logger.LogInfo($"Setup GUI in {sw.Elapsed.Milliseconds} ms");
        }

        [Obsolete("Use Core.UI.LegacyGUIManager")]
        public static event Action OnLoad
        {
            add => Core.UI.LegacyGUIManager.OnLoad += value;
            remove => Core.UI.LegacyGUIManager.OnLoad -= value;
        }

        [Obsolete("Use Core.UI.LegacyGUIManager")]
        public static event Action<bool> OnUIToggle
        {
            add => Core.UI.LegacyGUIManager.OnUIToggle += value;
            remove => Core.UI.LegacyGUIManager.OnUIToggle -= value;
        }

        [Obsolete("Use Core.UI.LegacyGUIManager")]
        public static GameObject GUI
        {
            get => Core.UI.LegacyGUIManager.GUI;
            set => Core.UI.LegacyGUIManager.GUI = value;
        }

        [Obsolete("Use Core.UI.LegacyGUIManager")]
        public static GameObject UserInterface
        {
            get => Core.UI.LegacyGUIManager.UserInterface;
            set => Core.UI.LegacyGUIManager.UserInterface = value;
        }

        [Obsolete("Use Core.UI.LegacyGUIManager")]
        public static GameObject Pinned
        {
            get => Core.UI.LegacyGUIManager.Pinned;
            set => Core.UI.LegacyGUIManager.Pinned = value;
        }

        [Obsolete("Use Core.UI.LegacyGUIManager")]
        public static bool Showing
        {
            get => Core.UI.LegacyGUIManager.Showing;
            set => Core.UI.LegacyGUIManager.Showing = value;
        }

        public static void GUIBind(this ConfigEntry<bool> entry, Toggle toggle)
        {
            entry.SettingChanged += ((EventHandler)((sender, args) => toggle.Set(entry.Value, false))).Invoke();
            toggle.On(state => entry.Value = state);
        }

        public static void GUIBind(this ConfigEntry<float> entry, BoundSlider slider)
        {
            entry.SettingChanged += ((EventHandler)((sender, args) => slider.slider.Set(entry.Value))).Invoke();
            slider.slider.On(state => entry.Value = state);
        }

        public class BoundSlider
        {
            public Text text;
            public Slider slider;

            public static BoundSlider Create(Transform obj) => Create(obj.GetComponent<Slider>(), obj.GetComponent<Text>());

            public static BoundSlider Create(Slider slider, Text text)
            {
                BoundSlider s = new();
                s.slider = slider;
                s.text = text;

                if (text != null)
                {
                    string format = text.text;
                    slider.onValueChanged.AddListener(new Action<float>(value => text.text = string.Format(format, value)));
                }

                return s;
            }
        }
    }
}
