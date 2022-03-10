using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Reflection;
using UnityEngine.SceneManagement;
using VRC.SDKBase;
using VRC.Udon.Wrapper.Modules;

namespace KiraiMod.Modules
{
    public static class Pickups
    {
        private static readonly HarmonyMethod NoOp = typeof(Pickups).GetMethod(nameof(HkNoOp)).ToHM();

        static Pickups()
        {
            typeof(Throwing).Initialize();
            typeof(Theft).Initialize();
        }

        private static class Theft
        {
            public static ConfigEntry<bool> Enabled = Shared.Config.Bind("Pickups", "Theft", true, "Should you be able to take pickups out of other people's hands");

            private static readonly MethodInfo orig = typeof(ExternVRCSDK3ComponentsVRCPickup).GetMethod(nameof(ExternVRCSDK3ComponentsVRCPickup.__set_DisallowTheft__SystemBoolean));
            private static MethodInfo hook;

            static Theft() =>
                Enabled.SettingChanged += ((EventHandler)((sender, args) =>
                {
                    if (Enabled.Value)
                    {
                        Events.WorldLoaded += OnWorldLoaded;
                        SetAll();
                        hook = Shared.Harmony.Patch(orig, NoOp);
                    }
                    else
                    {
                        Events.WorldLoaded -= OnWorldLoaded;
                        Shared.Harmony.Unpatch(orig, hook);
                    }
                })).Invoke();

            private static void OnWorldLoaded(Scene scene) => SetAll();
            private static void SetAll()
            {
                foreach (VRC_Pickup pickup in UnityEngine.Object.FindObjectsOfType<VRC_Pickup>())
                    pickup.DisallowTheft = false;
            }
        }

        private static class Throwing
        {
            public static ConfigEntry<bool> Enabled = Shared.Config.Bind("Pickups", "Throwing", true, "Should pickup throw speed be modified");
            public static ConfigEntry<float> Speed = Shared.Config.Bind("Pickups", "ThrowSpeed", 5.0f, "The speed at which pickups are thrown");

            private static readonly MethodInfo orig = typeof(ExternVRCSDK3ComponentsVRCPickup).GetMethod(nameof(ExternVRCSDK3ComponentsVRCPickup.__set_ThrowVelocityBoostScale__SystemSingle));
            private static MethodInfo hook;

            static Throwing()
            {
                Enabled.SettingChanged += ((EventHandler)((sender, args) => {
                    if (Enabled.Value)
                    {
                        Events.WorldLoaded += OnWorldLoaded;
                        SetAll();
                        hook = Shared.Harmony.Patch(orig, NoOp);
                    }
                    else
                    {
                        Events.WorldLoaded -= OnWorldLoaded;
                        Shared.Harmony.Unpatch(orig, hook);
                    }
                })).Invoke();

                Speed.SettingChanged += (sender, args) =>
                {
                    if (!Enabled.Value) return;
                    float speed = Speed.Value;
                    foreach (VRC_Pickup pickup in UnityEngine.Object.FindObjectsOfType<VRC_Pickup>())
                        pickup.ThrowVelocityBoostScale = speed;
                };
            }

            private static void OnWorldLoaded(Scene scene) => SetAll();
            private static void SetAll()
            {
                float speed = Speed.Value;
                foreach (VRC_Pickup pickup in UnityEngine.Object.FindObjectsOfType<VRC_Pickup>())
                    pickup.ThrowVelocityBoostScale = speed;
            }
        }

        private static bool HkNoOp() => false;
    }
}
