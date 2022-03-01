using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace KiraiMod.Loader
{
    [BepInPlugin("me.kiraihooks.KiraiMod.Loader", "KM.Loader", "0.0.0")]
    public class Loader : BasePlugin
    {
        public const string Server = "https://KiraiHooks.me/KiraiMod/Public/";
        private const string Updater = "KiraiMod.Updater.exe";

        internal static ManualLogSource Logger;
        internal static HttpClient http = new();

        public override void Load()
        {
            Logger = Log;

            if (Config.Bind("Updating", "Enabled", true, "Should the loader attempt to auto update").Value)
                VersionTable.OnFetched += UpdateFiles;

            VersionTable.Fetch(
                Config.Bind("Updating", "Safe", false, "Updating will halt if the server cannot be contacted").Value,
                Config.Bind("Updating", "Slow", false, "Enabling will prevent the process from moving forward until the server responds").Value
            );
        }

        private void UpdateFiles(Dictionary<string, string> table)
        {
            string cd = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/";
            string[] toUpdate = FetchOutOfDate(table, cd);

            foreach (string file in toUpdate)
                Log.LogDebug($"Scheduling for update: {file}");

            if (toUpdate.Length > 0)
            {
                if (!File.Exists(cd + Updater))
                    File.WriteAllBytes(cd + Updater, http.GetByteArrayAsync(Server + Updater).Result);

                Process.Start(cd + Updater, $"{string.Join("\\", toUpdate)}");
                Process.GetCurrentProcess().Kill();
            }
        }

        private string[] FetchOutOfDate(Dictionary<string, string> table, string cd) => 
            Directory.EnumerateFiles(cd, "KiraiMod.*")
                .Select(x => (x, SHA256(Encoding.UTF8.GetBytes(Path.GetFileName(x) + ".hash"))))
                .Where(x => table.ContainsKey(x.Item2))
                .Select(x => (x.x, SHA256(File.ReadAllBytes(x.x)), table[x.Item2]))
                .Where(x =>
                {
                    if (x.Item2 == x.Item3) return false;

                    string fname = Path.GetFileName(x.x);
                    if (fname != Updater) return true;
                    else
                    {
                        File.WriteAllBytes(x.x, http.GetByteArrayAsync(Server + fname).Result);
                        return false;
                    }
                })
                .Select(x => Path.GetFileName(x.x))
                .ToArray();

        private string SHA256(byte[] bytes) => string.Join("", new SHA256Managed().ComputeHash(bytes).Select(x => string.Format("{0:x2}", x)).ToArray());
    }
}
