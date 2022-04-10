using UnityEngine;
using VRC.SDKBase;
using BepInEx.Configuration;

namespace KiraiMod.Modules
{
    public static class Tracers
    {
        public static ConfigEntry<bool> PlayerTracers = Plugin.cfg.Bind(nameof(Tracers), "Players", false, "Should lines be drawn to every player in the world");

        private static LineRenderer lr;

        static Tracers()
        {
            GUI.Groups.Loaded += () => GUI.Groups.Visuals.AddElement("Player Tracers", PlayerTracers.Value).Bound.Bind(PlayerTracers);

            Events.ApplicationStart += () =>
            {
                SetupLineRenderer(lr = new GameObject("KiraiMod.Tracers").DontDestroyOnLoad().AddComponent<LineRenderer>(), new Color32(0x56, 0x00, 0xA5, 0xAF));
                PlayerTracers.SettingChanged += ((System.EventHandler)((sender, args) =>
                {
                    if (lr.gameObject.active = PlayerTracers.Value)
                        Events.Update += UpdatePlayerTracers;
                    else Events.Update -= UpdatePlayerTracers;
                })).Invoke();
            };
        }

        public static void UpdatePlayerTracers()
        {
            if (Networking.LocalPlayer is null) return;

            lr.positionCount = VRCPlayerApi.AllPlayers.Count * 2;
            Draw(lr, Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position);
        }

        private static void SetupLineRenderer(LineRenderer lr, Color color)
        {
            lr.material = new Material(Shader.Find("Hidden/Internal-Colored"));
            lr.startWidth = 0.002f;
            lr.endWidth = 0.002f;
            lr.useWorldSpace = false;
            lr.endColor = color;
            lr.startColor = color;
            lr.sortingOrder = -10;
            lr.sortingLayerID = 7711475;
            lr.material.SetInt("_ZWrite", 1.0);
            lr.material.SetInt("_ZTest", 0.0);
        }

        private static void Draw(LineRenderer lr, Vector3 src)
        {
            for (int i = 0; i < VRCPlayerApi.AllPlayers.Count; i++)
            {
                lr.SetPosition(i * 2, src);
                lr.SetPosition(i * 2 + 1, VRCPlayerApi.AllPlayers[i].gameObject.transform.position);
            }
        }
    }
}
