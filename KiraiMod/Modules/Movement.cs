using BepInEx.Configuration;
using HarmonyLib;
using KiraiMod.Core.UI;
using KiraiMod.Core.Utils;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using VRC.SDKBase;

namespace KiraiMod.Modules
{
    public static class Movement
    {
        public static readonly Bound<bool> state = new();
        public static readonly ConfigEntry<bool> noclip /**/ = Plugin.cfg.Bind("Flight", "NoClip", false, " Should you be able to go through solid objects");
        public static readonly ConfigEntry<bool> directional = Plugin.cfg.Bind("Flight", "Directional", false, "Should you move in the direction you are looking");
        public static readonly ConfigEntry<float> speed /**/ = Plugin.cfg.Bind("Flight", "Speed", 8.0f, "The speed in meters per second at which you fly");
        public static readonly ConfigEntry<Key[]> keybind = Plugin.cfg.Bind("Flight", "keybind", new Key[] { Key.LeftCtrl, Key.F }, "The keybind to toggle flight");

        static Movement()
        {
            Events.WorldUnloaded += scene => state.Value = false;

            state.ValueChanged += value => {
                if (value) Flight.Enable();
                else Flight.Disable();
            };

            keybind.Register(() => state.Value = !state._value);

            noclip.SettingChanged += (sender, args) => Collisions.Set(noclip.Value);
            directional.SettingChanged += (sender, args) => Target.Fetch();

            typeof(Hooks).Initialize();

            LegacyGUIManager.OnLoad += () =>
            {
                UIGroup ui = new(nameof(Movement));
                ui.RegisterAsHighest();

                UIGroup flight = new("Flight", ui);
                flight.AddElement("Enabled", state);
                flight.AddElement("Flight Speed", speed.Value).Bound.Bind(speed);
                flight.AddElement("NoClip", noclip.Value).Bound.Bind(noclip);
                flight.AddElement("Directional", directional.Value).Bound.Bind(directional);

                UIGroup position = new("Position", ui);
                position.AddElement("Save Position").Changed += SavePosition;
                position.AddElement("Load Position").Changed += LoadPosition;

                ui.AddElement("Use Legacy Locomotion").Changed += UseLegacyLocomotion;
            };
        }

        // in the future this may have more complicated logic
        public static void UseLegacyLocomotion() => Networking.LocalPlayer.UseLegacyLocomotion();

        private static Vector3 prevPos;
        public static void SavePosition() => prevPos = Networking.LocalPlayer.gameObject.transform.position;
        public static void LoadPosition() => Networking.LocalPlayer.gameObject.transform.position = prevPos;

        private static class Flight
        {
            public static Vector3 oGrav = new(0, -9.8f, 0);

            public static void Enable()
            {
                Physics.gravity = Vector3.zero;

                if (XRDevice.isPresent)
                    Events.Update += UpdateVR;
                else Events.Update += UpdateDesktop;
                Events.WorldLoaded += WorldLoaded;
                if (noclip.Value)
                    Collisions.Set(true);
                Target.Fetch();
            }

            public static void Disable()
            {
                if (XRDevice.isPresent)
                    Events.Update -= UpdateVR;
                else Events.Update -= UpdateDesktop;
                Events.WorldLoaded -= WorldLoaded;
                Collisions.Set(false);

                Physics.gravity = oGrav;
            }

            private static void WorldLoaded(UnityEngine.SceneManagement.Scene scene) => Events.Update += UpdateCheck;

            private static void UpdateCheck()
            {
                if (Networking.LocalPlayer == null)
                    return;

                Events.Update -= UpdateCheck;

                if (noclip.Value)
                    Collisions.Set(true);
                Target.Fetch();
            }

            private static void UpdateDesktop()
            {
                if (Networking.LocalPlayer == null) return;

                unsafe
                {
                    byte shift = ToByte(Input.GetKey(KeyCode.LeftShift));
                    float _speed = speed.Value;

                    Networking.LocalPlayer.gameObject.transform.position +=
                        Target.value.forward * _speed * Time.deltaTime *
                            (ToByte(Input.GetKey(KeyCode.W)) + ~ToByte(Input.GetKey(KeyCode.S)) + 1) * (shift * 8 + 1)
                        + Target.value.right * _speed * Time.deltaTime *
                            (ToByte(Input.GetKey(KeyCode.D)) + ~ToByte(Input.GetKey(KeyCode.A)) + 1) * (shift * 8 + 1)
                        + Target.value.up * _speed * Time.deltaTime *
                            (ToByte(Input.GetKey(KeyCode.E)) + ~ToByte(Input.GetKey(KeyCode.Q)) + 1) * (shift * 8 + 1);
                }

                Networking.LocalPlayer.SetVelocity(new Vector3(0f, 0f, 0f));
            }

            private static void UpdateVR()
            {
                if (Networking.LocalPlayer == null) return;
                float _speed = speed.Value;

                Networking.LocalPlayer.gameObject.transform.position +=
                    Target.value.forward * _speed * Time.deltaTime * Input.GetAxis("Vertical")
                    + Target.value.right * _speed * Time.deltaTime * Input.GetAxis("Horizontal")
                    + Target.value.up * _speed * Time.deltaTime * Input.GetAxis("Oculus_CrossPlatform_SecondaryThumbstickVertical");

                Networking.LocalPlayer.SetVelocity(new Vector3(0f, 0f, 0f));
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            private static unsafe byte ToByte(bool from) => *(byte*)&from;
        }

        private static class Collisions
        {
            private static Collider collider;

            public static void Set(bool state)
            {
                if (collider == null)
                {
                    if (Networking.LocalPlayer == null) return;
                    collider = Networking.LocalPlayer.gameObject.GetComponent<Collider>();
                }

                collider.enabled = !state;
            }
        }

        private static class Target
        {
            public static Transform value;

            public static void Fetch()
            {
                if (Networking.LocalPlayer == null)
                    return;

                value = directional.Value
                    ? GameObject.Find("_Application/TrackingVolume/TrackingSteam(Clone)/SteamCamera/[CameraRig]/Neck/Camera (head)").transform
                    : Networking.LocalPlayer.gameObject.transform;
            }
        }

        private static class Hooks
        {
            static Hooks() => Harmony.CreateAndPatchAll(typeof(Hooks));

            [HarmonyPrefix, HarmonyPatch(typeof(Physics), nameof(Physics.gravity), MethodType.Setter)]
            public static bool Hook_set_gravity(Vector3 __0)
            {
                if (__0.magnitude == 0)
                    return true;

                Flight.oGrav = __0;
                return !state._value;
            }
        }
    }
}
