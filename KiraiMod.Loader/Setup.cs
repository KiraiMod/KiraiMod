using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace KiraiMod.Loader
{
    internal static class Setup
    {
        public const string Server = "https://KiraiHooks.me/KiraiMod/Extensions/";

        public static void Run(HttpClient http, ManualLogSource log)
        {
            string path = $"{Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)}/Extensions/";
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

                Task<string> task = http.GetStringAsync(Server + "Info");
            task.Wait();
            if (task.IsFaulted)
            {
                log.LogError(task.Exception);
                Console.WriteLine("Failed to contact the server.\nPress enter to continue...");
                Console.ReadLine();
                return;
            }

            (string, string)[] infos = task.Result.Split('\n')
                .Select(x => x.Split('=')).Select(x => (x.ElementAt(0), string.Join("=", x.Skip(1))))
                .ToArray();

            int count = (int)Math.Log10(infos.Length) + 1;
            for (int i = 0; i < infos.Length; i++)
                Console.WriteLine($"[{(i + 1).ToString().PadLeft(count)}] {infos[i].Item1}: {infos[i].Item2}");

            bool hasInstalled = false;

            Console.WriteLine("Make sure you trust the developer of the extension", ConsoleColor.Red);
            for (; ; )
            {
                Console.WriteLine("Enter your selection or [q]uit: ");
                string input = Console.ReadLine().Trim().ToLower();
                if (input.Contains("q")) break;
                if (!int.TryParse(input, out int selection))
                {
                    Console.WriteLine("Invalid input");
                    continue;
                }

                if (selection <= 0 || selection > infos.Length)
                {
                    Console.WriteLine("Selection out of bounds");
                    continue;
                }

                string toInstall = infos[selection - 1].Item1;
                Console.WriteLine("Installing extension " + toInstall);
                Task<byte[]> taskb = http.GetByteArrayAsync(Server + toInstall);
                taskb.Wait();
                if (taskb.IsFaulted)
                {
                    log.LogError(taskb.Exception);
                    Console.WriteLine("An exception occurred whilst trying to download extension");
                    continue;
                }

                File.WriteAllBytes(path + toInstall, taskb.Result);
                hasInstalled = true;
            }

            if (hasInstalled) Environment.Exit(0);
        }
    }
}
