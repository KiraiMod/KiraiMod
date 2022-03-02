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
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;

namespace KiraiMod.Loader
{
    [BepInPlugin("me.kiraihooks.KiraiMod.Loader", "KM.Loader", "0.0.0")]
    public class Loader : BasePlugin
    {
        public const string Server = "https://KiraiHooks.me/KiraiMod/";
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

            if ((GetAsyncKeyState('K') & 0x8000) > 0)
            {
                Log.LogMessage("Entering setup utility...");
                Console.Beep();
                Setup.Run(http, Log);
                Log.LogInfo("Setup utility finished");
            }
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
            Directory.EnumerateFiles(cd, "*", SearchOption.AllDirectories)
                .Select(x => (path: x, kname: GetKName(cd, x)))
                .Select(x => (x.path, x.kname, nameHash: SHA256(Encoding.UTF8.GetBytes(x.kname + ".hash"))))
                .Where(x => table.ContainsKey(x.nameHash))
                .Select(x => (x.path, x.kname, oldHash: SHA256(File.ReadAllBytes(x.path)), newHash: table[x.nameHash]))
                .Where(x => x.oldHash != x.newHash)
                .Where(x =>
                {
                    if (x.kname != "Public/" + Updater) return true;
                    else
                    {
                        File.WriteAllBytes(x.path, http.GetByteArrayAsync(Server + x.kname).Result);
                        return false;
                    }
                })
                .Select(x => x.kname)
                .ToArray();

        private string SHA256(byte[] bytes) => string.Join("", new SHA256Managed().ComputeHash(bytes).Select(x => string.Format("{0:x2}", x)).ToArray());

        [DllImport("user32.dll")]
        private static extern int GetAsyncKeyState(int vKey);

        private static string GetKName(string cd, string path)
        {
            string rel = GetRelativePath(cd, path);
            int slashIdx = rel.IndexOf("\\");
            if (slashIdx == -1 || slashIdx == rel.Length - 1) return $"Public/{rel}";
            else return rel.Replace('\\', '/');
        }

        private static string GetRelativePath(string from, string to)
        {
            Uri _from = new(from);
            Uri _to = new(to);

            if (_from.Scheme != _to.Scheme) return to;

            Uri relativeUri = _from.MakeRelativeUri(_to);
            string relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            if (string.Equals(_to.Scheme, Uri.UriSchemeFile, StringComparison.OrdinalIgnoreCase))
                return relativePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
            else return relativePath;
        }
    }
}
