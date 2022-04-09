using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using VRC.Core;

namespace KiraiMod.Managers
{
    public static class WorldManager
    {
        public static readonly List<(string, Type)> SceneNameMods = new();
        public static readonly List<(string, Type)> WorldIDMods = new();
        public static object[] LoadedWorldMods;

        private static Scene currentScene;

        static WorldManager()
        {
            Events.WorldLoaded += scene => currentScene = scene;
            Events.WorldUnloaded += OnWorldUnloaded;
            Events.WorldInstanceLoaded += OnInstanceLoaded;
        }

        private static void OnInstanceLoaded(ApiWorldInstance instance)
        {
            LoadedWorldMods = SceneNameMods.Where(x => x.Item1 == currentScene.name)
                .Concat(WorldIDMods.Where(x => x.Item1 == instance.world.id))
                .Select(x => x.Item2)
                .Distinct()
                .Select(target =>
                {
                    try { return Activator.CreateInstance(target); }
                    catch (Exception ex)
                    {
                        Plugin.log.LogError($"Failed to load world mod: {ex}");
                        return null;
                    }
                })
                .Where(x => x != null)
                .ToArray();
        }

        private static void OnWorldUnloaded(Scene scene) => LoadedWorldMods = null;
    }
}
