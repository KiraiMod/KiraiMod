using System;
using System.Linq;
using System.Reflection;
using UnityEngine;
using VRC.Core;

namespace KiraiMod.Types
{
    public static class RoomManager
    {
        public static PropertyInfo m_CurrentWorld;

        static RoomManager()
        {
            m_CurrentWorld = AppDomain.CurrentDomain.GetAssemblies()
               .First(f => f.GetName().Name == "Assembly-CSharp")
               .GetExportedTypes()
               .Where(x => x.BaseType == typeof(MonoBehaviour))
               .Where(x => x.GetMethod("OnConnectedToMaster") != null)
               .SelectMany(x => x.GetProperties(BindingFlags.Public | BindingFlags.Static))
               .FirstOrDefault(x => x.PropertyType == typeof(ApiWorldInstance));

            if (m_CurrentWorld is null)
                Shared.Logger.LogWarning("Failed to find the RoomManager");
            else Shared.Logger.LogDebug($"RoomManager={m_CurrentWorld.DeclaringType.Name}");
        }

        public static ApiWorldInstance GetCurrentWorld() => (ApiWorldInstance)m_CurrentWorld.GetValue(null);
    }
}
