using KiraiMod.Core.UI;
using KiraiMod.Core.Utils;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace KiraiMod.Modules
{
    public static class Downloads
    {
        public static Bound<bool> paused = new();

        private static MonoBehaviour downloadManager;

        static Downloads()
        {
            GUI.Groups.Loaded += () =>
            {
                UIGroup ui = new("Downloads", GUI.Groups.Protections);
                ui.AddElement("Pause", paused);
                //ui.AddElement("Clear").Changed += ClearDownloads;
            };

            Events.ApplicationStart += () => downloadManager = GameObject.Find("_Application/AssetBundleDownloadManager").GetComponents<MonoBehaviour>()[0];

            paused.ValueChanged += state => downloadManager.enabled = !state;
        }
    }
}
