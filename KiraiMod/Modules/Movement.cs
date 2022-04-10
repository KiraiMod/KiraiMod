using KiraiMod.Core.UI;
using UnityEngine;
using VRC.SDKBase;

namespace KiraiMod.Modules
{
    public static class Movement
    {
        static Movement()
        {
            GUI.Groups.Loaded += () =>
            {
                UIGroup position = new("Position", GUI.Groups.Movement);
                position.AddElement("Save Position").Changed += SavePosition;
                position.AddElement("Load Position").Changed += LoadPosition;

                GUI.Groups.Movement.AddElement("Use Legacy Locomotion").Changed += UseLegacyLocomotion;
            };
        }

        // in the future this may have more complicated logic
        public static void UseLegacyLocomotion() => Networking.LocalPlayer.UseLegacyLocomotion();

        private static Vector3 prevPos;
        public static void SavePosition() => prevPos = Networking.LocalPlayer.gameObject.transform.position;
        public static void LoadPosition() => Networking.LocalPlayer.gameObject.transform.position = prevPos;
    }
}
