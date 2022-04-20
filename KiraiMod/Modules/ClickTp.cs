using BepInEx.Configuration;
using KiraiMod.Core.UI;
using System;
using UnityEngine;
using VRC.SDKBase;

namespace KiraiMod.Modules
{
    public static class ClickTp
    {
        public static ConfigEntry<bool> clickTp = Plugin.cfg.Bind("ClickTp", "Toggle", false, "Should you be able to Tp using left ctrl and left mouse click");
        public static ConfigEntry<int> tpRange = Plugin.cfg.Bind("ClickTp", "Tp Range", 100000, "How far should you be able to Tp");
        public static Camera cameraTarget;
        static ClickTp()
        {
            Events.UIManagerLoaded += () =>
            {
                cameraTarget = Camera.main;
            };
            GUI.Groups.Loaded += () =>
            {
                UIGroup ClickTp = new("ClickTp", GUI.Groups.Movement);
                ClickTp.AddElement("ClickTp", clickTp.Value).Bound.Bind(clickTp);
            };
            clickTp.SettingChanged += ((EventHandler)((sender, args) =>
            {
                Events.Update += () => RaycastTp();
            })).Invoke();
        }
        public static void RaycastTp()
        {
            RaycastHit hit;
            if (!clickTp.Value) return;
            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyUp(KeyCode.Mouse0))
            {
                Physics.Raycast(cameraTarget.transform.position, cameraTarget.transform.forward, out hit, tpRange.Value);
                if (hit.transform == null) return;
                Networking.LocalPlayer.gameObject.transform.position = hit.point;
            };
        }
    }
}