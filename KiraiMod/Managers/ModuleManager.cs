using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace KiraiMod.Managers
{
    public static class ModuleManager
    {
        public static event Action<Dictionary<string, Type[]>> OnModuleRegistered;

        public static Dictionary<string, Type[]> Modules = new();

        // in the future this class may be responsible for UI stuff, a la old kiraimod
        static ModuleManager()
        {
            Register(Assembly.GetExecutingAssembly());

            foreach (Type module in Modules["KiraiMod.Modules"])
                if (!module.IsNested)
                    module.Initialize();
        }

        public static void Register() => Register(Assembly.GetCallingAssembly());
        public static void Register(Assembly assembly)
        {
            Dictionary<string, Type[]> module = assembly.GetExportedTypes()
                .GroupBy(x => x.Namespace)
                .ToDictionary(x => x.Key, x => x.ToArray());

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
