using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace KiraiMod.Managers
{
    public static class ModuleManager
    {
        public static event Action<Dictionary<string, Type[]>> OnModuleRegistered;

        public static Dictionary<string, Type[]> Modules = new();

        // in the future this class may be responsible for UI stuff, a la old kiraimod
        // with how Core.UI is looking, probably not, but its still possible
        static ModuleManager()
        {
            Register(Assembly.GetExecutingAssembly());

            foreach (Type module in Modules["Modules"])
                if (!module.IsNested)
                {
                    Plugin.log.LogDebug("Loading " + module.FullName);
                    try { module.Initialize(); } 
                    catch (Exception ex) { Plugin.log.LogError("Exception occurred whilst loading " + module.FullName + ": " + ex); }
                }
        }

        public static void Register() => Register(Assembly.GetCallingAssembly());
        public static void Register(Assembly assembly)
        {
            Regex r = new($"^{assembly.GetName().Name}[.]?");

            Dictionary<string, Type[]> module = assembly.GetExportedTypes()
                .GroupBy(x => x.Namespace)
                .ToDictionary(x => r.Replace(x.Key, ""), x => x.ToArray());

            module.ToList()
                .ForEach(x =>
                {
                    /// x.Key = namespace
                    if (Modules.TryGetValue(x.Key, out Type[] types))
                        Modules[x.Key] = types.Concat(x.Value).ToArray();
                    else Modules.Add(x.Key, x.Value);
                });

            OnModuleRegistered?.Invoke(module);
        }
    }
}
