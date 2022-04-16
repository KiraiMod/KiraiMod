using UnityEngine;

namespace KiraiMod.Modules
{
    public static class Skybox
    {
        public static Material material;

        static Skybox()
        {
            GUI.Groups.Loaded += () => GUI.Groups.Visuals.AddElement("Clear Skybox").Changed += ClearSkybox;

            Events.ApplicationStart += () =>
            {
                material = new(Shader.Find("Unlit/Color"));
                material.color = Color.black;
            };
        }

        public static void ClearSkybox() => RenderSettings.skybox = material;
    }
}
