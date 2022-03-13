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
        public static List<object> LoadedWorldMods = new();

        static WorldManager()
        {
            Events.WorldLoaded += OnWorldLoaded;
            Events.WorldUnloaded += OnWorldUnloaded;
        }

        private static void OnWorldLoaded(Scene scene)
        {
            ApiWorldInstance current = Core.Types.RoomManager.GetCurrentWorld();

            LoadedWorldMods = SceneNameMods.Where(x => x.Item1 == scene.name)
                .Concat(WorldIDMods.Where(x => x.Item1 == current.worldId))
                .Select(x => x.Item2)
                .Distinct()
                .Select(target =>
                {
                    try { return Activator.CreateInstance(target); }
                    catch (Exception ex)
                    {
                        Shared.Logger.LogError($"Failed to load world mod: {ex}");
                        return null;
                    }
                })
                .Where(x => x != null)
                .ToList();
        }

        private static void OnWorldUnloaded(Scene scene) => LoadedWorldMods.Clear();
    }
}
