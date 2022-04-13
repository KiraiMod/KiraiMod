using UnityEngine;
using VRC.SDKBase;
using BepInEx.Configuration;
using UnhollowerRuntimeLib;
using System;

namespace KiraiMod.Modules
{
    public static class Tracers
    {
        public static ConfigEntry<bool> PlayerTracers = Plugin.cfg.Bind("Tracers", "Players", false, "Should lines be drawn to every player in the world");

        public static GameObject tracers;
        public static TracersHelper instance;
        public static Camera cameraMain;

        static Tracers() 
        {
            Events.ApplicationStart += () => 
            {
                tracers = new GameObject("KiraiMod.Tracers").DontDestroyOnLoad();
                instance = tracers.AddComponent<TracersHelper>();
                PlayerTracers.SettingChanged += ((EventHandler)((sender, args) => instance.enabled = PlayerTracers.Value)).Invoke();
            };

            Events.UIManagerLoaded += () => cameraMain = Camera.main;

            GUI.Groups.Loaded += () => GUI.Groups.Visuals.AddElement("Player Tracers", PlayerTracers.Value).Bound.Bind(PlayerTracers);
        }

        public class TracersHelper : MonoBehaviour
        {
            static TracersHelper() => ClassInjector.RegisterTypeInIl2Cpp<TracersHelper>();

            public TracersHelper() : base(ClassInjector.DerivedConstructorPointer<TracersHelper>()) => ClassInjector.DerivedConstructorBody(this);
            public TracersHelper(IntPtr ptr) : base(ptr) { }
            
            public void OnRenderObject()
            {
                if (Networking.LocalPlayer == null || Camera.current != cameraMain) return;
                
                // this can be optimized but it seems to be going null randomly
                Material mat = new(Shader.Find("Hidden/Internal-Colored"));
                mat.SetInt("_ZTest", 0);
                mat.SetColor("Color", new Color(0x56, 0x00, 0xA5));
                mat.renderQueue = 5000;
                mat.SetPass(0);

                Vector3 local = Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position;

                foreach (VRCPlayerApi player in VRCPlayerApi.AllPlayers)
                {
                    GL.PushMatrix();
                    GL.Begin(1);
                    // todo: make this color based on user info
                    GL.Color(GUI.Colors.Primary);
                    GL.Vertex(player.gameObject.transform.position);
                    GL.Vertex(local);
                    GL.End();
                    GL.PopMatrix();
                }
            }
        }
    }
}
