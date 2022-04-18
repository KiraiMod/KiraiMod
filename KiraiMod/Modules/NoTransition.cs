using BepInEx.Configuration;
using System;
using UnityEngine;

namespace KiraiMod.Modules
{
    // the idea for this comes from that person that was friends with keafy
    // their mod had an issue, i think it was causing the game to crash
    public static class NoTransition
    {
        public static ConfigEntry<bool> Enabled = Plugin.cfg.Bind("Visuals", "No Transitions", true, "Should the black fade between worlds be removed?");

        public static GameObject Fade;

        static NoTransition()
        {
            Events.UIManagerLoaded += () =>
            {
                Fade = GameObject.Find("UserInterface/PlayerDisplay/BlackFade/inverted_sphere");

                Enabled.SettingChanged += ((EventHandler)((sender, args) => Fade.active = !Enabled.Value)).Invoke();
            };
        }
    }
}
