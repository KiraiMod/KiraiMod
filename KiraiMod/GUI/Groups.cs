using KiraiMod.Core.UI;
using System;
using System.Collections.Generic;
using System.Text;

namespace KiraiMod.GUI
{
    public static class Groups
    {
        public static event Action Loaded;

        public static UIGroup Movement;
        public static UIGroup Visuals;
        public static UIGroup Protections;

        static Groups()
        {
            LegacyGUIManager.OnLoad += () =>
            {
                (Movement = new(nameof(Movement))).RegisterAsHighest();
                (Visuals = new(nameof(Visuals))).RegisterAsHighest();
                (Protections = new(nameof(Protections))).RegisterAsHighest();

                Loaded?.StableInvoke();
            };
        }
    }
}
