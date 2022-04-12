using BepInEx.Configuration;
using System;
using System.Reflection;
using UnityEngine;
using VRC.Core;
using VRC.SDKBase;

namespace KiraiMod.Modules
{
    public static class ESP
    {
        public static ConfigEntry<bool> Enabled = Plugin.cfg.Bind("ESP", "Enabled", true, "Should players have an outline drawn around them?");

        private static readonly MethodInfo m_GetSimpleAvatarPedestal = typeof(Component).GetMethod(nameof(Component.GetComponent), new Type[0]).MakeGenericMethod(Core.Types.SimpleAvatarPedestal.Type);

        static ESP()
        {
            GUI.Groups.Loaded += () => GUI.Groups.Visuals.AddElement(nameof(ESP), Enabled.Value).Bound.Bind(Enabled);

            Core.Utils.ToggleHook hook = new(
                typeof(PipelineManager).GetMethod(nameof(PipelineManager.Start)),
                null,
                typeof(ESP).GetMethod(nameof(ESP.OnAvatarLoaded), BindingFlags.NonPublic | BindingFlags.Static)
            );

            Enabled.SettingChanged += ((EventHandler)((sender, args) =>
            {
                hook.Toggle(Enabled.Value);
                SetAll(Enabled.Value);
            })).Invoke();
        }

        private static void OnAvatarLoaded(PipelineManager __instance)
        {
            if (__instance.contentType != PipelineManager.ContentType.avatar
                || Networking.GetOwner(__instance.gameObject).isLocal
                || m_GetSimpleAvatarPedestal.Invoke(__instance.transform.parent, null) != null)
                return;

            Set(__instance.transform, true);
        }

        private static void Set(Transform transform, bool state)
        {
            foreach (Renderer renderer in transform.GetComponentsInChildren<Renderer>(true))
                if (renderer.bounds.extents.x <= 1.5 || renderer.bounds.extents.y <= 1.5 || renderer.bounds.extents.z <= 1.5)
                    Core.Types.HighlightsFX.Highlight(renderer, state);
        }

        private static void SetAll(bool state)
        {
            foreach (VRCPlayerApi player in VRCPlayerApi.AllPlayers)
                if (!player.isLocal)
                    Set(player.gameObject.transform.Find("ForwardDirection/Avatar"), state);
        }
    }
}
