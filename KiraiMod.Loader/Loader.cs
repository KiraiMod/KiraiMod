using BepInEx;
using BepInEx.IL2CPP;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KiraiMod.Loader
{
    [BepInPlugin("me.kiraihooks.KiraiMod.Loader", "KM.Loader", "0.0.0")]
    public class Loader : BasePlugin
    {
        public const string Server = "https://KiraiHooks.me/KiraiMod/Public/";
        private const string Updater = "KiraiMod.Updater.exe";

        private readonly HttpClient http = new();

        private bool safeLoad;

        public override void Load()
        {
            if (!Config.Bind("Updating", "Enabled", true, "Should the loader attempt to auto update").Value) return;
            safeLoad = Config.Bind("Updating", "Safe", false, "Updating will halt if the server cannot be contacted").Value;
            bool slowLoad = Config.Bind("Updating", "Slow", false, "Enabling will prevent the process from moving forward until the server responds").Value;

            Task<string> task = http.GetStringAsync(Server + "Info");
            task.ContinueWith(ProcessInfo);

            if (slowLoad) task.Wait();
        }

        private void ProcessInfo(Task<string> task)
        {
            if (task.IsFaulted)
            {
                Log.LogError(task.Exception);
                if (safeLoad)
                {
                    Log.LogFatal($"Failed to contact the server and SafeLoad was enabled");
                    Process.GetCurrentProcess().Kill();
                    Environment.Exit(0);
                    for (; ; );
                }
                else Log.LogWarning("Unable to contact the remote server for updates");
                return;
            }

#if DEBUG
            Log.LogDebug("Resolving version table");
#endif
            Dictionary<string, string> table = task.Result.ToCharArray()
                .Where(x => !char.IsUpper(x))
                .Select((chr, index) => (chr, index))
                .GroupBy(x => x.index / 128, x => x.chr)
                .Select(x => string.Join("", x.ToArray()))
                .ToDictionary(x => x.Substring(0, 64), x => x.Substring(64));

            string cd = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/";
            List<string> toUpdate = new();

#if DEBUG
            Log.LogDebug("Enumerating files");
#endif
            Directory.EnumerateFiles(cd, "KiraiMod.*")
                .Select(x => (x, SHA256(Encoding.UTF8.GetBytes(Path.GetFileName(x) + ".hash"))))
                .Where(x => table.ContainsKey(x.Item2))
                .Select(x => (x.x, SHA256(File.ReadAllBytes(x.x)), table[x.Item2]))
                .Where(x => x.Item2 != x.Item3)
                .ToList()
                .ForEach(data =>
                {
                    string fname = Path.GetFileName(data.x);
#if DEBUG
                    Log.LogDebug("Updating " + fname);
#endif
                    if (fname == Updater) File.WriteAllBytes(data.x, http.GetByteArrayAsync(Server + fname).Result);
                    else toUpdate.Add(fname);
                });

            if (toUpdate.Count > 0)
            {
                if (!File.Exists(cd + Updater))
                    File.WriteAllBytes(cd + Updater, http.GetByteArrayAsync(Server + Updater).Result);

                Process.Start(cd + Updater, $"{string.Join("\\", toUpdate)}");
                Process.GetCurrentProcess().Kill();
            }
        }

        private string SHA256(byte[] bytes) => string.Join("", new SHA256Managed().ComputeHash(bytes).Select(x => string.Format("{0:x2}", x)).ToArray());
    }
}
