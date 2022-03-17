using System.Collections.Generic;
using UnityEngine;
using VRC.Core;
using VRC.SDKBase;

namespace KiraiMod.Modules
{
    public static class Players
    {
        public static List<Core.Types.Player> Selected = new();
        public static Core.Types.Player Target;

        static Players()
        {
            Events.PlayerJoined += OnPlayerJoined;
            Events.PlayerLeft += OnPlayerLeft;
        }

        private static void OnPlayerJoined(Core.Types.Player player)
        {
            GUI.Players.Total.text = VRCPlayerApi.AllPlayers.Count.ToString();
            GUI.Players.CreatePlayer().Configure(player);
        }

        private static void OnPlayerLeft(Core.Types.Player player)
        {
            GUI.Players.Total.text = (VRCPlayerApi.AllPlayers.Count - 1).ToString();
            int offset = 0;

            if (Target == player)
                Target = null;

            for (int i = 0; i < GUI.Players.All.Count; i++)
            {
                var gp = GUI.Players.All[i];

                if (gp.player.Inner == player.Inner)
                {
                    GUI.Players.All.RemoveAt(i--);
                    gp.Self.Destroy();
                    offset++;
                } else gp.SetIndex(gp.Self.GetSiblingIndex() - offset);
            }
        }

        public static Color GetTrustColor(APIUser user)
        {
            if (user is null) return RankColors.Unknown;

            if (user.hasLegendTrustLevel)
            {
                if (user.tags.Contains("system_legend"))
                    return RankColors.Legendary;
                return RankColors.Veteran;
            }
            else if (user.hasVeteranTrustLevel) return RankColors.Trusted;
            else if (user.hasTrustedTrustLevel) return RankColors.Known;
            else if (user.hasKnownTrustLevel) return RankColors.User;
            else if (user.hasBasicTrustLevel) return RankColors.NewUser;
            else if (user.isUntrusted)
            {
                if (user.tags.Contains("system_probable_troll"))
                    return RankColors.Nuisance;
                return RankColors.Visitor;
            }
            else return RankColors.Unknown;
        }

        public static class RankColors
        {
            public static Color32 Legendary = new(0xFF, 0x69, 0xB4, 0xFF);
            public static Color32 Veteran = new(0xFF, 0xD0, 0x00, 0xFF);
            public static Color32 Trusted = new(0xB1, 0x8F, 0xFF, 0xFF);
            public static Color32 Known = new(0xFF, 0x7B, 0x42, 0xFF);
            public static Color32 User = new(0x2B, 0xCF, 0x5C, 0xFF);
            public static Color32 NewUser = new(0x17, 0x78, 0xFF, 0xFF);
            public static Color32 Visitor = new(0xCC, 0xCC, 0xCC, 0xFF);
            public static Color Nuisance = new(0.47f, 0.18f, 0.18f);
            public static Color32 Unknown = new(0x20, 0x20, 0x20, 0xFF);
        }
    }
}
