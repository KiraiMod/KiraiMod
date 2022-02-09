using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace KiraiMod.Managers
{
    public static class KeybindManager
    {
        private static bool prevRS = false;

        static KeybindManager()
        {
            Shared.Logger.LogInfo("starting keybind manager");
            InputSystem.add_onEvent((Il2CppSystem.Action<InputEventPtr, InputDevice>)OnEvent);
        }

        // this will always run less than OnUpdate would've
        // makes more sense to only check keybinds when keys are modified
        private static void OnEvent(InputEventPtr ptr, InputDevice dev)
        {
            Keyboard kb = dev.TryCast<Keyboard>();
            if (kb == null) return;

            if (kb.rightShiftKey.isPressed && !prevRS)
            {
                prevRS = true;
                GUIManager.UserInterface.gameObject.active ^= true;
            } else prevRS = false;
        }
    }
}
