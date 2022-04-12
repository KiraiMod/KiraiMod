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
        //public static Camera cameraMain;

        static Tracers() 
        {
            Events.ApplicationStart += () => 
            { 
                //cameraMain = Camera.main; 
                CreateGOTracers(); 
            };
            GUI.Groups.Loaded += () => GUI.Groups.Visuals.AddElement("Player Tracers", PlayerTracers.Value).Bound.Bind(PlayerTracers);
        }

        public static void CreateGOTracers()
        {
            tracers = new GameObject("KiraiMod.Tracers").DontDestroyOnLoad();
            instance = tracers.AddComponent<TracersHelper>();
            PlayerTracers.SettingChanged += ((EventHandler)((sender, args) =>
            {
                instance.enabled = PlayerTracers.Value;
            })).Invoke();

        }
        public class TracersHelper : MonoBehaviour
        {
            static TracersHelper() => ClassInjector.RegisterTypeInIl2Cpp<TracersHelper>();
            public TracersHelper() : base(ClassInjector.DerivedConstructorPointer<TracersHelper>()) => ClassInjector.DerivedConstructorBody(this);
            public TracersHelper(IntPtr ptr) : base(ptr) { }

            private static Material lineMaterial;
            public void OnRenderObject()
            {
                if (Networking.LocalPlayer == null) return;
                if (Camera.current != Camera.main) return; //can be optimized by caching Camera.main & if statments can be collapsed into 1
                foreach (VRCPlayerApi player in VRCPlayerApi.AllPlayers)
                {
                    GL.PushMatrix();
                    CreateLineMaterial();
                    lineMaterial.SetPass(0);
                    GL.Begin(1);
                    GL.Color(new Color(0x56, 0x00, 0xA5, 1));
                    if (player.gameObject.transform.Find("ForwardDirection/Avatar") == null) return;
                    GL.Vertex(player.gameObject.transform.Find("ForwardDirection/Avatar").position);
                    GL.Vertex(Networking.LocalPlayer.GetTrackingData(VRCPlayerApi.TrackingDataType.RightHand).position);
                    GL.End();
                    GL.PopMatrix();
                }
            }
            static void CreateLineMaterial()
            {
                lineMaterial = new Material(Shader.Find("Hidden/Internal-Colored"));
                lineMaterial.SetInt("_ZTest", 0);
                lineMaterial.SetColor("Color", new Color(0x56, 0x00, 0xA5));
                lineMaterial.renderQueue = 5000;

            }
        }
    }

}