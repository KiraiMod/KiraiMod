using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Reflection;

namespace KiraiMod.Modules
{
    public static class ForceClone
    {
        public static ConfigEntry<bool> Enabled = Plugin.cfg.Bind("ForceClone", "Enabled", true, "Force a user to allow you to clone their avatar if it is public");

        private static readonly MethodInfo original = typeof(VRC.Core.APIUser).GetProperty(nameof(VRC.Core.APIUser.allowAvatarCopying)).GetSetMethod();
        private static readonly HarmonyMethod target = typeof(ForceClone).GetMethod(nameof(Hook), BindingFlags.NonPublic | BindingFlags.Static).ToHM();
        private static MethodInfo hook;

        public static bool _state;
        public static bool State
        {
            set
            {
                if (_state == value) return;
                _state = value;

                if (value) hook = Plugin.harmony.Patch(original, target);
                else Plugin.harmony.Unpatch(original, hook);
            }
        }

        static ForceClone() => Enabled.SettingChanged += ((EventHandler)((sender, args) => State = Enabled.Value)).Invoke();

        private static void Hook(ref bool __0) => __0 = true;
    }
}
