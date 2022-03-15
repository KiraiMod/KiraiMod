﻿using BepInEx.Configuration;
using ExitGames.Client.Photon;
using KiraiMod.Core;
using System;
using System.Reflection;
using UnityEngine.SceneManagement;

namespace KiraiMod.Voice.Modules
{
    public static class Voice
    {
        public static ConfigEntry<bool> UtopiaVoice = Plugin.cfg.Bind("Voice", "UtopiaVoice", false, "Prevent from hearing you unless they have Utopia Voice");
        public static ConfigEntry<bool> UtopiaOnly = Plugin.cfg.Bind("Voice", "UtopiaOnly", false, "Only hear other Utopia voices");

        private static readonly MethodInfo m_MicVolume = Core.Types.USpeaker.Type.GetProperty("field_Internal_Static_Single_1").GetSetMethod();
        private static readonly object[] volumeOn = new object[1] { float.MaxValue };
        private static readonly object[] volumeOff = new object[1] { 1 };

        public static bool _loudmic;
        public static bool LoudMic
        {
            set
            {
                if (_loudmic == value) return;
                _loudmic = value;
                GUI.Voice.LoudMic.Set(value, false);

                if (value)
                {
                    m_MicVolume.Invoke(null, volumeOn);
                    Events.WorldLoaded += OnWorldLoad;
                }
                else
                {
                    m_MicVolume.Invoke(null, volumeOff);
                    Events.WorldLoaded -= OnWorldLoad;
                }
            }
        }

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
        }

        private static void OnWorldLoad(Scene scene) => LoudMic = false;

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
    }
}
