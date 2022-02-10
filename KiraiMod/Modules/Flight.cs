using KiraiMod.Managers;
using UnityEngine.InputSystem;

namespace KiraiMod.Modules
{
    public static class Flight
    {
        static Flight()
        {
            Shared.Config.Bind(
                "Flight",
                "keybind",
                new Key[] { Key.LeftCtrl, Key.F },
                "The keybind to toggle flight"
            ).RegisterKeybind(() => State ^= true);
        }

        private static bool state;
        public static bool State
        {
            get => state;
            set {
                if (state == value) return;
                state = value;

                if (value) Enable();
                else Disable();
            }
        }

        private static void Enable()
        {
            Shared.Logger.LogInfo("Flight on");
        }

        private static void Disable()
        {
            Shared.Logger.LogInfo("Flight off");
        }
    }
}
