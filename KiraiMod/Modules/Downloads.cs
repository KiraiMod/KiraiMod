using KiraiMod.Core.UI;
using KiraiMod.Core.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace KiraiMod.Modules
{
    public static class Downloads
    {
        public static Bound<bool> paused = new();

        private static MonoBehaviour downloadManager;
        private static readonly PropertyInfo[] queues = Core.Types.DownloadManager.Type.GetProperties()
            .Where(x => x.PropertyType.IsGenericType && x.PropertyType.GetGenericTypeDefinition() == typeof(Il2CppSystem.Collections.Generic.Queue<>))
            .ToArray();

        // in the future, this should use the inner type of all queues instead of the first one
        private static readonly MethodInfo clear = typeof(Il2CppSystem.Collections.Generic.Queue<>).MakeGenericType(queues[0].PropertyType.GenericTypeArguments[0]).GetMethod("Clear");

        static Downloads()
        {
            GUI.Groups.Loaded += () =>
            {
                UIGroup ui = new("Downloads", GUI.Groups.Protections);
                ui.AddElement("Pause", paused);
                ui.AddElement("Clear").Changed += Clear;
            };

            Events.ApplicationStart += () => downloadManager = (MonoBehaviour)Core.Types.DownloadManager.Type.Singleton().GetValue(null);

            paused.ValueChanged += state => downloadManager.enabled = !state;
        }

        // todo: print the amount of downloads cleared
        public static void Clear() => queues.ForEach(queue => clear.Invoke(queue.GetValue(downloadManager), null));
    }
}
