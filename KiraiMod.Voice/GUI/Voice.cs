using KiraiMod.Core;
using KiraiMod.GUI.Common;
using KiraiMod.Managers;
using UnityEngine;
using UnityEngine.UI;

namespace KiraiMod.Voice.GUI
{
    public static class Voice
    {
        public static Toggle LoudMic;

        public static void Setup(Transform Self)
        {
            Transform Body = Self.Find("Body");

            Window.Create(Self)
                .Dragable()
                .Closable(Body.gameObject)
                .Pinnable();

            (LoudMic = Body.Find("LoudMic").GetComponent<Toggle>()).On(state => Modules.Voice.LoudMic = state);
            Modules.Voice.UtopiaVoice.GUIBindFull(Body.Find("UtopiaVoice").GetComponent<Toggle>());
            Modules.Voice.UtopiaOnly.GUIBindFull(Body.Find("UtopiaOnly").GetComponent<Toggle>());
        }
    }
}
