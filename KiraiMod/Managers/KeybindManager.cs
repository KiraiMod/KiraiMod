using BepInEx.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace KiraiMod.Managers
{
    public static class KeybindManager
    {
        public static Dictionary<string, Keybind> binds = new();

        static KeybindManager()
        {
            TomlTypeConverter.AddConverter(typeof(Key[]), new TypeConverter()
            {
                ConvertToObject = (str, type) => str.Substring(1, str.Length - 2).Split(',').Select(x => (Key)Enum.Parse(typeof(Key), x.Trim())).ToArray(),
                ConvertToString = (obj, type) => $"[{string.Join(", ", (obj as Key[]).Select(x => Enum.GetName(typeof(Key), x)))}]",
            });

            InputSystem.add_onEvent((Il2CppSystem.Action<InputEventPtr, InputDevice>)OnEvent);
        }

        // this will always run less than OnUpdate would've
        // makes more sense to only check keybinds when keys are modified
        private static void OnEvent(InputEventPtr ptr, InputDevice dev)
        {
            Keyboard kb = dev.TryCast<Keyboard>();
            if (kb == null) return;

            foreach (Keybind bind in binds.Values)
            {
                int kc = 0;
                foreach (Key key in bind.keys)
                    if (kb[key].isPressed)
                        kc++;

                if (bind.previous)
                {
                    if (kc != bind.keys.Length)
                        bind.previous = false;
                } 
                else if (kc == bind.keys.Length)
                {
                    bind.previous = true;
                    bind.OnClick();
                }
            }
        }

        public static void RegisterKeybind(this ConfigEntry<Key[]> entry, Action OnClick)
        {
            Keybind bind = new()
            {
                key = $"{entry.Definition.Section}::{entry.Definition.Key}",
                keys = entry.Value,
                OnClick = OnClick
            };

            entry.SettingChanged += (sender, args) => bind.keys = entry.Value;

            binds[bind.key] = bind;
        }

        public class Keybind
        {
            public string key;
            public bool previous;
            public Key[] keys;
            public Action OnClick;
        }
    }
}
