using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KiraiMod.Modules
{
    public static class Flight
    {
        private static bool state;
        public static bool State
        {
            get => state;
            set {
                if (state == value) return;
                state = value;

                if (value) Enable();
                else Disable();
            }
        }

        private static void Enable()
        {
            Shared.Logger.LogInfo("Flight on");
        }

        private static void Disable()
        {
            Shared.Logger.LogInfo("Flight off");
        }
    }
}
