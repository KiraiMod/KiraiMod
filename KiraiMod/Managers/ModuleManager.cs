using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace KiraiMod.Managers
{
    // this needs to be rewritten eventually
    public static class ModuleManager
    {
        public static event Action<Dictionary<string, Type[]>> OnModuleRegistered;

        public static Dictionary<string, Action<Type>[]> Subscriptions = new(); 
        public static Dictionary<string, Type[]> Modules = new();

        // in the future this class may be responsible for UI stuff, a la old kiraimod
        // with how Core.UI is looking, probably not, but its still possible
        static ModuleManager()
        {
            Register(Assembly.GetExecutingAssembly());

            Subscribe("Modules", module =>
            {
                if (!module.IsNested)
                {
                    Plugin.log.LogDebug("Loading " + module.FullName);
                    try { module.Initialize(); }
                    catch (Exception ex) { Plugin.log.LogError("Exception occurred whilst loading " + module.FullName + ": " + ex); }
                }
            });
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

                    if (Subscriptions.TryGetValue(x.Key, out Action<Type>[] subs))
                        x.Value.ForEach(type => subs.ForEach(sub => sub(type)));
                });

            OnModuleRegistered?.Invoke(module);
        }

        public static void Subscribe(string module, Action<Type> action)
        {
            if (action is null) throw new ArgumentNullException(nameof(action));

            Modules[module].ForEach(action);

            if (Subscriptions.TryGetValue(module, out Action<Type>[] types))
                Subscriptions[module] = types.Append(action).ToArray();
            else Subscriptions.Add(module, new Action<Type>[1] { action });
        }
    }
}
