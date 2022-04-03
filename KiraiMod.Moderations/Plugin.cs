using BepInEx;
using BepInEx.IL2CPP;
using BepInEx.Logging;
using ExitGames.Client.Photon;
using HarmonyLib;
using KiraiMod.Core;
using System;
using System.Reflection;

namespace KiraiMod.Moderations
{
    [BepInPlugin(GUID, "KM.M9s", "0.1.0")]
    [BepInDependency(Core.Plugin.GUID)]
    public class Plugin : BasePlugin
    {
        public const string GUID = "me.kiraihooks.KiraiMod.Moderations";

        private readonly Harmony harmony = new(GUID);
        internal static ManualLogSource logger;

        public override void Load()
        {
            logger = Log;

            harmony.Patch(
                Core.Types.VRCNetworkingClient.m_OnEvent,
                typeof(Plugin).GetMethod(nameof(Hook_OnEvent), BindingFlags.NonPublic | BindingFlags.Static).ToHM()
            );
        }

        private static bool Hook_OnEvent(EventData __0)
        {
            if (__0.Code != 33) return true;

            var dict = __0.CustomData.Cast<Il2CppSystem.Collections.Generic.Dictionary<byte, Il2CppSystem.Object>>();
            switch ((ModerationOpCode)dict[0].Unbox<byte>())
            {
                case ModerationOpCode.VoteKick:
                    logger.LogMessage("A vote kick has been started");
                    break;

                case ModerationOpCode.BlockMute:
                    if (dict.Count == 4) // single player
                        return OnBlockMute(dict[1].Unbox<int>(), dict[10].Unbox<bool>(), dict[11].Unbox<bool>());
                    break;
            }

            return true;
        }

        private static bool OnBlockMute(int actor, bool k10, bool k11)
        {
            VRC.SDKBase.VRCPlayerApi player = VRC.SDKBase.VRCPlayerApi.AllPlayers.Find(
                UnhollowerRuntimeLib.DelegateSupport.ConvertDelegate<Il2CppSystem.Predicate<VRC.SDKBase.VRCPlayerApi>>(
                    new Predicate<VRC.SDKBase.VRCPlayerApi>(x => x.playerId == actor)
                )
            );

            if (player == null) return true;

            logger.LogMessage($"{player.displayName} {(k10 ? "block" : k11 ? "mut" : "revert")}ed you");

            return !k10;
        }

        enum ModerationOpCode : byte
        {
            Warning = 2,
            ModeratorMute = 8,
            Friends = 10,
            VoteKick = 13,
            __unk0 = 20,
            BlockMute = 21
        }
    }
}
