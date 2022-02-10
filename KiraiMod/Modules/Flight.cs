using BepInEx.Configuration;
using HarmonyLib;
using KiraiMod.Managers;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using VRC.SDKBase;

namespace KiraiMod.Modules
{
    public static class Flight
    {
        public static readonly ConfigEntry<bool> directional = Shared.Config.Bind(
            "Flight",
            "Directional",
            false,
            "Should you move in the direction you are looking"
        );

        public static readonly ConfigEntry<bool> noclip = Shared.Config.Bind(
            "Flight",
            "NoClip",
            false,
            "Should you be able to go through solid objects"
        );

        public static readonly ConfigEntry<float> speed = Shared.Config.Bind(
            "Flight",
            "Speed",
            8.0f,
            "The speed in meters per second at which you fly"
        );

        private static bool state;
        public static bool State
        {
            get => state;
            set
            {
                if (state == value) return;
                GUI.Movement.flight.Set(state = value, false);

                if (value) Enable();
                else Disable();
            }
        }

        private static Collider collider;

        static Flight()
        {
            Shared.Config.Bind(
                "Flight",
                "keybind",
                new Key[] { Key.LeftCtrl, Key.F },
                "The keybind to toggle flight"
            ).RegisterKeybind(() => State ^= true);

            noclip.SettingChanged += ((System.EventHandler)((sender, args) => {
                bool val = noclip.Value;
                if (val) Events.WorldLoaded += WorldLoaded;
                else Events.WorldLoaded -= WorldLoaded;

                if (collider == null)
                {
                    if (Networking.LocalPlayer == null) return;
                    collider = Networking.LocalPlayer.gameObject.GetComponent<Collider>();
                }

                collider.enabled = !val;
            })).Invoke();

            Harmony.CreateAndPatchAll(typeof(Flight));
        }

        private static void WorldLoaded(UnityEngine.SceneManagement.Scene scene)
        {
            // TODO: redo noclip if enabled;
        }

        private static Vector3 oGrav = new(0, -9.8f, 0);

        private static void Enable()
        {
            Physics.gravity = Vector3.zero;

            if (XRDevice.isPresent)
                Events.Update += UpdateVR;
            else Events.Update += UpdateDesktop;

        }

        private static void Disable()
        {
            if (XRDevice.isPresent)
                Events.Update -= UpdateVR;
            else Events.Update -= UpdateDesktop;

            Physics.gravity = oGrav;
        }

        private static void UpdateDesktop()
        {
            if (Networking.LocalPlayer is null) return;

            unsafe
            {
                byte shift = ToByte(Input.GetKey(KeyCode.LeftShift));
                float _speed = speed.Value;

                Networking.LocalPlayer.gameObject.transform.position +=
                    Networking.LocalPlayer.gameObject.transform.forward * _speed * Time.deltaTime *
                        (ToByte(Input.GetKey(KeyCode.W)) + ~ToByte(Input.GetKey(KeyCode.S)) + 1) * (shift * 8 + 1)
                    + Networking.LocalPlayer.gameObject.transform.right * _speed * Time.deltaTime *
                        (ToByte(Input.GetKey(KeyCode.D)) + ~ToByte(Input.GetKey(KeyCode.A)) + 1) * (shift * 8 + 1)
                    + Networking.LocalPlayer.gameObject.transform.up * _speed * Time.deltaTime *
                        (ToByte(Input.GetKey(KeyCode.E)) + ~ToByte(Input.GetKey(KeyCode.Q)) + 1) * (shift * 8 + 1);
            }

            Networking.LocalPlayer.SetVelocity(new Vector3(0f, 0f, 0f));
        }

        private static void UpdateVR()
        {
            if (Networking.LocalPlayer is null) return;
            float _speed = speed.Value;

            Networking.LocalPlayer.gameObject.transform.position +=
                Networking.LocalPlayer.gameObject.transform.forward * _speed * Time.deltaTime * Input.GetAxis("Vertical")
                + Networking.LocalPlayer.gameObject.transform.right * _speed * Time.deltaTime * Input.GetAxis("Horizontal")
                + Networking.LocalPlayer.gameObject.transform.up * _speed * Time.deltaTime * Input.GetAxis("Oculus_CrossPlatform_SecondaryThumbstickVertical");

            Networking.LocalPlayer.SetVelocity(new Vector3(0f, 0f, 0f));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static unsafe byte ToByte(bool from) => *(byte*)&from;

        [HarmonyPrefix, HarmonyPatch(typeof(Physics), nameof(Physics.gravity), MethodType.Setter)]
        internal static bool Hook_set_gravity(Vector3 __0)
        {
            if (__0.magnitude == 0)
                return true;

            oGrav = __0;
            return !state;
        }
    }
}
