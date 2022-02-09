using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace KiraiMod.Managers
{
    public static class ModuleManager
    {
        public static Dictionary<string, Type[]> Modules;

        // in the future this class may be responsible for UI stuff, a la old kiraimod
        static ModuleManager()
        {
            Modules = Assembly.GetExecutingAssembly()
                .GetExportedTypes()
                .GroupBy(x => x.Namespace)
                .ToDictionary(x => x.Key, x => x.ToArray());

            foreach (Type module in Modules["KiraiMod.Modules"]) 
                module.Initialize();
        }
    }
}
