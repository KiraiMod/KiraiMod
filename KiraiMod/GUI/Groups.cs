using KiraiMod.Core.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace KiraiMod.GUI
{
    public static class Groups
    {
        public static event Action Loaded;

        public static UIGroup Visuals;

        static Groups()
        {
            LegacyGUIManager.OnLoad += () =>
            {
                (Visuals = new(nameof(Visuals))).RegisterAsHighest();

                Loaded?.StableInvoke();
            };
        }
    }
}
