using BepInEx.Configuration;
using KiraiMod.Core.UI;
using System;
using UnityEngine;
using VRC.SDKBase;

namespace KiraiMod.Modules
{
    public static class ClickTeleport
    {
        public static ConfigEntry<bool> enabled = Plugin.cfg.Bind("Movement", "ClickTeleport", false, "Should you be able to teleport using left ctrl and left mouse click");
        public static ConfigEntry<int> range = Plugin.cfg.Bind("Movement", "ClickTeleport Range", 100000, "How far should you be able to teleport");
        public static Camera camera;

        static ClickTeleport()
        {
            Events.UIManagerLoaded += () => camera = Camera.main;

            GUI.Groups.Loaded += () =>
            {
                UIGroup ui = new("ClickTp", GUI.Groups.Movement);
                ui.AddElement("ClickTp", clickTp.Value).Bound.Bind(clickTp);
            };

            enabled.SettingChanged += ((EventHandler)((sender, args) =>
            {
                if (enabled.Value)
                    Events.Update += OnUpdate;
                else Events.Update -= OnUpdate;
            })).Invoke();
        }

        public static void OnUpdate()
        {
            RaycastHit hit;
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyUp(KeyCode.Mouse0))
            {
                Physics.Raycast(camera.transform.position, camera.transform.forward, out hit, range.Value);
                if (hit.transform == null) return;
                Networking.LocalPlayer.gameObject.transform.position = hit.point;
            };
        }
    }
}