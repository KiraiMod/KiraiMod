using HarmonyLib;
using System;
using System.Linq;

namespace KiraiMod.Modules
{
    // TODO: rewrite this
    // TODO: add paranoid mode where it will abort if the new id isn't exact
    public static class HWID
    {
        private static readonly string target;

        static HWID()
        {
            if (!Shared.Config.Bind("HWID", "Enabled", true, string.Empty).Value)
                return;

            string old = UnityEngine.SystemInfo.deviceUniqueIdentifier;

            Shared.Logger.LogInfo("Old HWID: " + old);

            var config = Shared.Config.Bind<string>("HWID", "Target", null, "The target hardware ID that you will be spoofed to.\nSet empty for a new one.");
            target = config.Value;
            if (string.IsNullOrWhiteSpace(target))
                config.Value = target = Generate(old.Length);

            Shared.Logger.LogInfo("New HWID: " + target);

            if (target.Length != old.Length)
                Abort("Target HWID length does not match the original HWID length");

            Harmony.CreateAndPatchAll(typeof(HWID));

            if (UnityEngine.SystemInfo.deviceUniqueIdentifier == old)
                Abort("HWID is still the original after spoofing");
            else Shared.Logger.LogMessage("Successfully spoofed HWID");
        }

        public static string Generate(int length) => string.Join("", Enumerable.Range(0, length >> 1).Select(x => UnityEngine.Random.Range(0, 255).ToString("x2")));

        private static void Abort(string message)
        {
            Shared.Logger.LogFatal(message);
            Console.Beep();
            Console.ReadKey();
            Environment.Exit(0);
        }

        [HarmonyPostfix, HarmonyPatch(typeof(UnityEngine.SystemInfo), nameof(UnityEngine.SystemInfo.deviceUniqueIdentifier), MethodType.Getter)]
        internal static void Hook_get_deviceUniqueIdentifier(ref string __result) => __result = target;
    }
}
