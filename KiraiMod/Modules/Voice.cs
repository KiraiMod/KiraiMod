using BepInEx.Configuration;
using ExitGames.Client.Photon;
using KiraiMod.Core.Utils;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace KiraiMod.Modules
{
    public static class Voice
    {
        public static Bound<bool> LoudMic = new();
        public static ConfigEntry<bool> UtopiaVoice = Plugin.cfg.Bind("Voice", "UtopiaVoice", false, "Prevent from hearing you unless they have Utopia Voice");
        public static ConfigEntry<bool> UtopiaOnly = Plugin.cfg.Bind("Voice", "UtopiaOnly", false, "Only hear other Utopia voices");

        private static readonly MethodInfo m_MicVolume = Core.Types.USpeaker.Type.GetProperty("field_Internal_Static_Single_1").GetSetMethod();
        private static readonly object[] volumeOn = new object[1] { float.MaxValue };
        private static readonly object[] volumeOff = new object[1] { 1 };

        static Voice()
        {
            Core.UI.LegacyGUIManager.OnLoad += () =>
            {
                Core.UI.UIGroup ui = new(nameof(Voice));
                ui.RegisterAsHighest();
                ui.AddElement("Loud Mic", LoudMic);
                ui.AddElement("Utopia Voice", UtopiaVoice.Value).Bound.Bind(UtopiaVoice);
                ui.AddElement("Utopia Only", UtopiaOnly.Value).Bound.Bind(UtopiaOnly);
            };

            LoudMic.ValueChanged += value =>
            {
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
            };

            ToggleHook inbound = new ToggleHook(Core.Types.VRCNetworkingClient.m_OnEvent, typeof(Voice).GetMethod(nameof(HookInbound), BindingFlags.NonPublic | BindingFlags.Static)).Enable();
            ToggleHook outbound = new(Core.Types.VRCNetworkingClient.m_OpRaiseEvent, typeof(Voice).GetMethod(nameof(HookOutbound), BindingFlags.NonPublic | BindingFlags.Static));

            UtopiaVoice.SettingChanged += ((EventHandler)((sender, args) =>
            {
                outbound.Toggle(UtopiaVoice.Value);
                Indicator.SetHidden(UtopiaVoice.Value);
            })).Invoke();

            Core.UI.LegacyGUIManager.OnLoad += () =>
            {
                Indicator.image = GameObject.Find("UserInterface/UnscaledUI/HudContent/Hud/VoiceDotParent/VoiceDotDisabled").GetComponent<Image>();
                Indicator.SetHidden(UtopiaVoice.Value);
            };
        }

        private static void OnWorldLoad(Scene scene) => LoudMic.Value = false;

        private static unsafe void HookOutbound(byte __0, ref Il2CppSystem.Object __1)
        {
            if (__0 != 1) return;

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

        public static class Indicator
        {
            public static Image image;

            public static readonly Color originalColor = new(0.8456f, 0, 0, 0.5023f);
            public static readonly Color hiddenColor = new Color32(0x9B, 0xF4, 0x04, 0xFF);

            public static void SetHidden(bool state)
            {
                if (image is not null)
                    image.color = state ? hiddenColor : originalColor;
            }
        }
    }
}
