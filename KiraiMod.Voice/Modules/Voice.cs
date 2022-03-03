using BepInEx.Configuration;
using ExitGames.Client.Photon;
using KiraiMod.Core;
using System;
using System.Reflection;

namespace KiraiMod.Voice.Modules
{
    public static class Voice
    {
        public static ConfigEntry<bool> LoudMic = Plugin.cfg.Bind("Voice", "LoudMic", false);
        public static ConfigEntry<bool> UtopiaVoice = Plugin.cfg.Bind("Voice", "UtopiaVoice", false, "Prevent from hearing you unless they have Utopia Voice");
        public static ConfigEntry<bool> UtopiaOnly = Plugin.cfg.Bind("Voice", "UtopiaOnly", false, "Only hear other Utopia voices");

        static Voice()
        {
            Plugin.harmony.Patch(
                Core.Types.VRCNetworkingClient.m_OnEvent,
                typeof(Voice).GetMethod(nameof(HookInbound), BindingFlags.NonPublic | BindingFlags.Static).ToHM()
            );

            Plugin.harmony.Patch(
                Core.Types.VRCNetworkingClient.m_OpRaiseEvent,
                typeof(Voice).GetMethod(nameof(HookOutbound), BindingFlags.NonPublic | BindingFlags.Static).ToHM()
            );

            //LoudMic.SettingChanged += LoudMic_SettingChanged;
        }

        private static unsafe void HookOutbound(byte __0, ref Il2CppSystem.Object __1)
        {
            if (__0 != 1 || !UtopiaVoice.Value) return;

            var bytes = __1.Cast<UnhollowerBaseLib.Il2CppStructArray<byte>>();
            int* ptr = (int*)bytes.Pointer.ToPointer() + 9;
            *ptr = *ptr + 30_000;

            __1 = bytes.Cast<Il2CppSystem.Object>();
        }

        private static unsafe bool HookInbound(ref EventData __0)
        {
            if (__0.Code != 1) return true;

            var bytes = __0.CustomData.Cast<UnhollowerBaseLib.Il2CppStructArray<byte>>();
            int* ptr = (int*)bytes.Pointer.ToPointer() + 9;
            int age = Math.Abs(VRC.SDKBase.Networking.GetServerTimeInMilliseconds() - *ptr);

            if (age >= 25_000)
                *ptr -= 30_000;
            else return !UtopiaOnly.Value;

            __0.customData = bytes.Cast<Il2CppSystem.Object>();

            return true;
        }

        //private static void LoudMic_SettingChanged(object sender, EventArgs e) => USpeaker.field_Internal_Static_Single_1 = LoudMic.Value ? float.MaxValue : 1;
    }
}
