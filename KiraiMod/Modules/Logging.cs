using BepInEx.Configuration;
using System;

namespace KiraiMod.Modules
{
    public static class Logging
    {
        public static ConfigEntry<bool> PlayerJoin = Plugin.cfg.Bind("Logging", "PlayerJoin", true, "Should you be notified when someone joins?");
        public static ConfigEntry<bool> PlayerLeave = Plugin.cfg.Bind("Logging", "PlayerLeave", true, "Should you be notified when someone leaves?");

        static Logging()
        {
            PlayerJoin.SettingChanged += ((EventHandler)((sender, args) =>
            {
                if (PlayerJoin.Value) Events.Player.Joined += LogPlayerJoined;
                else Events.Player.Joined -= LogPlayerJoined;
            })).Invoke();

            PlayerLeave.SettingChanged += ((EventHandler)((sender, args) =>
            {
                if (PlayerLeave.Value) Events.Player.Left += LogPlayerLeft;
                else Events.Player.Left -= LogPlayerLeft;
            })).Invoke();
        }

        private static void LogPlayerJoined(Core.Types.Player player) => Plugin.log.LogMessage($"{player.APIUser.displayName} joined");
        private static void LogPlayerLeft(Core.Types.Player player) => Plugin.log.LogMessage($"{player.APIUser.displayName} left");
    }
}
