using BepInEx.Configuration;
using HarmonyLib;
using KiraiMod.Core.UI;
using KiraiMod.Core.Utils;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.SceneManagement;
using VRC.SDKBase;
using VRC.Udon.Wrapper.Modules;

namespace KiraiMod.Modules
{
    public static class Pickups
    {
        private static readonly MethodInfo NoOp = typeof(Pickups).GetMethod(nameof(HkNoOp), BindingFlags.NonPublic | BindingFlags.Static);

        static Pickups()
        {
            typeof(Modifiers).Initialize();
            typeof(Orbit).Initialize();

            LegacyGUIManager.OnLoad += () =>
            {
                UIGroup ui = new(nameof(Pickups));
                ui.RegisterAsHighest();

                ui.AddElement("Unlock", Modifiers.unlock.Value).Bound.Bind(Modifiers.unlock); //              if you think it's an anti pattern
                ui.AddElement("Theft", Modifiers.theft.Value).Bound.Bind(Modifiers.theft); //                 writing the name of the config entry
                ui.AddElement("Rotate", Modifiers.rotate.Value).Bound.Bind(Modifiers.rotate); //              twice for every single ui element
                ui.AddElement("Reach", Modifiers.reach.Value).Bound.Bind(Modifiers.reach); //                 then hope for bepinex to give
                ui.AddElement("Boost", Modifiers.boost.Value).Bound.Bind(Modifiers.boost); //                 config entry a base class we can use 
                ui.AddElement("Boost Speed", Modifiers.boostSpeed.Value).Bound.Bind(Modifiers.boostSpeed); // instead of bound

                ui.AddElement("Orbit", Orbit.State);
                ui.AddElement("Orbit Speed", Orbit.Speed.Value).Bound.Bind(Orbit.Speed);
                ui.AddElement("Orbit Distance", Orbit.Distance.Value).Bound.Bind(Orbit.Distance);
                ui.AddElement("Orbit Offset", Orbit.Offset.Value).Bound.Bind(Orbit.Offset);
            };
        }

        public static class Modifiers
        {
            public static ConfigEntry<bool> unlock = Shared.Config.Bind("Pickups", "Unlock", false, "Should all pickups always pickupable");
            public static ConfigEntry<bool> theft = Shared.Config.Bind("Pickups", "Theft", false, "Should you be able to take things out of people's hands");
            public static ConfigEntry<bool> rotate = Shared.Config.Bind("Pickups", "Rotate", false, "Shoud you be able to rotate all pickups in your hand");
            public static ConfigEntry<bool> reach = Shared.Config.Bind("Pickups", "Reach", false, "Should you be able to pickup things from any distance");
            public static ConfigEntry<bool> boost = Shared.Config.Bind("Pickups", "Boost", false, "Should you be able to throw things faster");
            public static ConfigEntry<float> boostSpeed = Shared.Config.Bind("Pickups", "BoostSpeed", 5.0f, "The speed at which pickups are thrown");

            static Modifiers()
            {
                Type type = typeof(ExternVRCSDK3ComponentsVRCPickup);
                new BasePickupModifier(type.GetMethod(nameof(ExternVRCSDK3ComponentsVRCPickup.__set_pickupable__SystemBoolean)), unlock, pickup => pickup.pickupable = true);
                new BasePickupModifier(type.GetMethod(nameof(ExternVRCSDK3ComponentsVRCPickup.__set_DisallowTheft__SystemBoolean)), theft, pickup => pickup.DisallowTheft = false);
                new BasePickupModifier(type.GetMethod(nameof(ExternVRCSDK3ComponentsVRCPickup.__set_allowManipulationWhenEquipped__SystemBoolean)), rotate, pickup => pickup.allowManipulationWhenEquipped = true);
                new BasePickupModifier(type.GetMethod(nameof(ExternVRCSDK3ComponentsVRCPickup.__set_proximity__SystemSingle)), reach, pickup => pickup.proximity = float.MaxValue);
                new BasePickupModifier(type.GetMethod(nameof(ExternVRCSDK3ComponentsVRCPickup.__set_ThrowVelocityBoostScale__SystemSingle)), boost, pickup => pickup.ThrowVelocityBoostScale = boostSpeed.Value);

                boostSpeed.SettingChanged += (sender, args) =>
                {
                    if (boost.Value)
                        foreach (VRC_Pickup pickup in UnityEngine.Object.FindObjectsOfType<VRC_Pickup>())
                            pickup.ThrowVelocityBoostScale = boostSpeed.Value;
                };

                Shared.Harmony.Patch(
                    typeof(VRC_Pickup).GetMethod(nameof(VRC_Pickup.Awake)),
                    typeof(Modifiers).GetMethod(nameof(HookAwake), BindingFlags.NonPublic | BindingFlags.Static).ToHM()
                );
            }

            private static void HookAwake(ref VRC_Pickup __instance)
            {
                if (unlock.Value) __instance.pickupable = true;
                if (theft.Value) __instance.DisallowTheft = false;
                if (rotate.Value) __instance.allowManipulationWhenEquipped = true;
                if (reach.Value) __instance.proximity = float.MaxValue;
                if (boost.Value) __instance.ThrowVelocityBoostScale = boostSpeed.Value;
            }

            public class BasePickupModifier
            {
                private readonly Action<VRC_Pickup> setup;

                public BasePickupModifier(MethodInfo orig, ConfigEntry<bool> entry, Action<VRC_Pickup> setup)
                {
                    this.setup = setup;
                    ToggleHook hook = new(orig, NoOp);

                    entry.SettingChanged += ((EventHandler)((sender, args) =>
                    {
                        if (theft.Value)
                        {
                            Events.WorldLoaded += OnWorldLoaded;
                            foreach (VRC_Pickup pickup in UnityEngine.Object.FindObjectsOfType<VRC_Pickup>())
                                setup(pickup);
                            hook.Toggle(true);
                        }
                        else
                        {
                            Events.WorldLoaded -= OnWorldLoaded;
                            hook.Toggle(false);
                        }
                    })).Invoke();
                }

                private void OnWorldLoaded(Scene scene)
                {
                    foreach (VRC_Pickup pickup in UnityEngine.Object.FindObjectsOfType<VRC_Pickup>())
                        setup(pickup);
                }
            }
        }

        public static class Orbit
        {
            public static ConfigEntry<float> Speed = Shared.Config.Bind("Pickups", "OrbitSpeed", 1f, "How fast the items complete a full revolution around the target");
            public static ConfigEntry<float> Distance = Shared.Config.Bind("Pickups", "OrbitDitance", 1f, "How far the items should be from the target");
            public static ConfigEntry<float> Offset = Shared.Config.Bind("Pickups", "OrbitOffset", 0f, "How much the items should be above or below the target");

            public static Bound<bool> State = new();

            private static VRC_Pickup[] pickups;

            static Orbit()
            {
                State.ValueChanged += value =>
                {
                    if (value)
                    {
                        pickups = UnityEngine.Object.FindObjectsOfType<VRC_Pickup>();
                        Events.Player.Left += CheckTargetMissing;
                        Events.Update += Update;
                    }
                    else
                    {
                        Events.Player.Left -= CheckTargetMissing;
                        Events.Update -= Update;
                    }
                };
            }

            private static void Update()
            {
                if (Players.Target == null)
                {
                    State.Value = false;
                    return;
                }

                float degrees = 360 / pickups.Length;

                for (int i = 0; i < pickups.Length; i++)
                {
                    VRC_Pickup pickup = pickups[i];

                    if (pickup is null)
                        continue;

                    if (Networking.GetOwner(pickup.gameObject) != Networking.LocalPlayer)
                        Networking.SetOwner(Networking.LocalPlayer, pickup.gameObject);

                    pickup.transform.position =
                        Players.Target.VRCPlayerApi.gameObject.transform.position
                        + new Vector3(Mathf.Sin(Time.time * Speed.Value + degrees * i) * Distance.Value, Offset.Value, Mathf.Cos(Time.time * Speed.Value + degrees * i) * Distance.Value);
                }
            }

            private static void CheckTargetMissing(Core.Types.Player player)
            {
                if (player.Inner == Players.Target.Inner)
                    State.Value = false;
            }
        }

        private static bool HkNoOp() => false;
    }
}
