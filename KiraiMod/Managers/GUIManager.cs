using BepInEx.Configuration;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace KiraiMod.Managers
{
    public static class GUIManager
    {
        public static event Action OnLoad;
        public static event Action<bool> OnUIToggle;

        public static GameObject GUI;
        public static GameObject UserInterface;
        public static GameObject Pinned;

        private static AssetBundle bundle;

        private static EventSystem system;
        private static BaseInputModule inputFix;
        private static BaseInputModule inputOrig;

        public static bool Showing
        {
            get => UserInterface.active;
            set
            {
                if (UserInterface.active == value)
                    return;

                UserInterface.active = value;
                OnUIToggle?.Invoke(value);

                inputOrig.enabled = !(inputFix.enabled = value);
            }
        }

        static GUIManager()
        {
            Events.UIManagerLoaded += OnUIManagerLoaded;

            Shared.Config.Bind("GUI", "Keybind", new Key[] { Key.RightShift }, "The keybind you want to use to open the GUI").Register(() => Showing ^= true);
        }

        private static void OnUIManagerLoaded()
        {
            SetupFix();
            LoadAssetBundle();
            CreateUI();
            SetupHandlers();
        }

        private static void SetupFix()
        {
            var sys = GameObject.Find("_Application/UiEventSystem");
            inputOrig = (system = sys.GetComponent<EventSystem>()).m_SystemInputModules[0];
            (inputFix = sys.AddComponent<StandaloneInputModule>()).enabled = false;
        }

        private static void LoadAssetBundle()
        {
            MemoryStream mem = new();
            Assembly.GetExecutingAssembly().GetManifestResourceStream("KiraiMod.Lib.KiraiMod.GUI.AssetBundle").CopyTo(mem);
            bundle = AssetBundle.LoadFromMemory(mem.ToArray());
            bundle.hideFlags |= HideFlags.HideAndDontSave;
        }

        private static void CreateUI()
        {
            (GUI = bundle.LoadAsset("assets/gui.prefab")
                .Cast<GameObject>()
                .Instantiate()
                .DontDestroyOnLoad()).name = "KiraiMod.GUI";

            UserInterface = GUI.transform.Find("UserInterface").gameObject;
            Pinned = GUI.transform.Find("Pinned").gameObject;

            try { OnLoad?.Invoke(); }  
            catch (Exception ex)
            {
                Shared.Logger.LogError("An exception has occurred whilst loading GUI " + ex);
            }

            Shared.Logger.LogInfo("Loaded GUI");
        }

        private static void SetupHandlers()
        {
            Stopwatch sw = new();
            sw.Start();

            Type[] handlers = ModuleManager.Modules["GUI"];

            for (int i = 0; i < UserInterface.transform.childCount; i++)
            {
                Transform child = UserInterface.transform.GetChild(i);
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
                } catch (Exception ex)
                {
                    Shared.Logger.LogError($"Setup method for GUI handler {child.name} failed: {ex}");
                    child.gameObject.active = false;
                }
            }

            UserInterface.active = false;

            sw.Stop();

            Shared.Logger.LogInfo($"Setup GUI in {sw.Elapsed.Milliseconds} ms");
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
