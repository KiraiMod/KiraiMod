using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KiraiMod.Loader
{
    public static class VersionTable
    {
        public static event Action<Dictionary<string, string>> OnFetched;

        private static bool safe;

        public static void Fetch(bool safe, bool slow)
        {
            VersionTable.safe = safe;

            Task<string> task = Loader.http.GetStringAsync(Loader.Server + "Info");
            task.ContinueWith(HandleResponse);

            if (slow) task.Wait();
        }

        private static void HandleResponse(Task<string> task)
        {
            if (task.IsFaulted)
            {
                Loader.Logger.LogError(task.Exception);
                if (safe)
                {
                    Loader.Logger.LogFatal($"Failed to contact the server and SafeLoad was enabled");
                    Process.GetCurrentProcess().Kill();
                    Environment.Exit(0);
                    for (; ; );
                }
                else Loader.Logger.LogWarning("Unable to contact the remote server for updates");
                return;
            }

            OnFetched(ResolveVersionTable(task.Result));
        }

        private static Dictionary<string, string> ResolveVersionTable(string data) =>
            data.ToCharArray()
                .Where(x => !char.IsUpper(x))
                .Select((chr, index) => (chr, index))
                .GroupBy(x => x.index / 128, x => x.chr)
                .Select(x => string.Join("", x.ToArray()))
                .ToDictionary(x => x.Substring(0, 64), x => x.Substring(64));
    }
}
