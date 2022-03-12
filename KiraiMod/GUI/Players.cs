using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KiraiMod.GUI
{
    public static class Players
    {
        public static List<Player> All = new();
        public static Player Current;

        public static Text Total;

        private static GameObject Template;

        public static void Setup(Transform Self)
        {
            Total = Self.Find("Title/Total").GetComponent<Text>();
            Transform Body = Self.Find("Body");

            Common.Window.Create(Self)
                .Dragable()
                .Closable(Body.gameObject)
                .Pinnable();

            Transform Scroll = Body.Find("Scroll");

            Template = Scroll.Find("Template").gameObject;
        }

        public static Player CreatePlayer()
        {
            Transform player = Template.Instantiate().transform;
            player.SetParent(Template.transform.parent);
            player.gameObject.active = true;
            return new Player(player);
        }

        public class Player
        {
            public static List<Player> Players = new();

            public Core.Types.Player player;

            public Transform Self;
            public Text Index;
            public Toggle Targeted;
            public Toggle Selected;
            public Text UsernameText;
            public Button Username;

            public Player(Transform Self)
            {
                this.Self = Self;
                Index = Self.Find("Index").GetComponent<Text>();
                Targeted = Self.Find("Targeted").GetComponent<Toggle>();
                Selected = Self.Find("Selected").GetComponent<Toggle>();

                Transform username = Self.Find("Username");

                UsernameText = username.GetComponent<Text>();
                Username = username.GetComponent<Button>();

                All.Add(this);
            }

            public void Configure(Core.Types.Player player)
            {
                this.player = player;

                SetIndex();
                UsernameText.text = player.APIUser.displayName;
                UsernameText.color = Modules.Players.GetTrustColor(player.APIUser);

                Username.On(() => Core.Types.UserSelectionManager.SelectUser(player.APIUser));

                Targeted.On(state =>
                {
                    Current?.Targeted.Set(false, false);
                    (Current, Modules.Players.Target) = state ? (this, player) : (null, null);
                });

                Selected.On(state =>
                {
                    if (state) Modules.Players.Selected.Add(player);
                    else Modules.Players.Selected.Remove(player);
                });
            }

            public void SetIndex(int? index = null) => Index.text = (index ?? Self.GetSiblingIndex()).ToString();
        }
    }
}
