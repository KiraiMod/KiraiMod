using BepInEx.Configuration;
using System;
using System.Collections;
using UnityEngine;

namespace KiraiMod.Modules
{
    public static class Logging
    {
        public static ConfigEntry<bool> PlayerJoin = Plugin.cfg.Bind("Logging", "PlayerJoin", true, "Should you be notified when someone joins?");
        public static ConfigEntry<bool> PlayerLeave = Plugin.cfg.Bind("Logging", "PlayerLeave", true, "Should you be notified when someone leaves?");
        public static ConfigEntry<bool> VotesReady = Plugin.cfg.Bind("Logging", "VotesReady", true, "Should you be notified when you can partake in votes?");

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

            VotesReady.SettingChanged += ((EventHandler)((sender, args) =>
            {
                if (PlayerLeave.Value) Events.WorldInstanceLoaded += StartVoteTimer;
                else Events.WorldInstanceLoaded -= StartVoteTimer;
            })).Invoke();
        }

        private static void LogPlayerJoined(Core.Types.Player player) => Plugin.log.LogMessage($"{player.APIUser.displayName} joined");
        private static void LogPlayerLeft(Core.Types.Player player) => Plugin.log.LogMessage($"{player.APIUser.displayName} left");
        private static void StartVoteTimer(VRC.Core.ApiWorldInstance _)
        {
            if (votesCoroutine != null)
            {
                votesCoroutine.Stop();
                votesCoroutine = null;
            }

            votesCoroutine = WaitForVotes().Start();
        }

        private static Coroutine votesCoroutine;
        private static readonly WaitForSeconds wait = new(300);
        private static IEnumerator WaitForVotes()
        {
            yield return wait;

            Plugin.log.LogMessage("You can now partake in votes");
        }
    }
}
