using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using VRC.Udon;

namespace KiraiMod.GUI
{
    // aka abbez
    public static class Manipulator
    {
        public static Transform Body;

        public static Toggle Repeater;
        public static Toggle Networked;
        public static Toggle Targeted;

        public static Text AlphaText;
        public static Text BetaText;
        public static Text GammaText;

        public static Button Alpha;
        public static Button Beta;
        public static Button Gamma;

        public static Button Broadcast;
        public static InputField BroadcastName;

        private static bool boundNetwork = false;

        public static void Setup(Transform self)
        {
            Body = self.Find(nameof(Body));
            Common.Window.Create(self, self.Find("Title"), Body)
                .Dragable()
                .Closable();

            (Repeater = Body.Find(nameof(Repeater)).GetComponent<Toggle>()).onValueChanged.AddListener(new Action<bool>(state => Modules.Manipulator.Repeater = state));

            // todo: rewrite this logic
            (Networked = Body.Find(nameof(Networked)).GetComponent<Toggle>()).onValueChanged.AddListener(new Action<bool>(state =>
            {
                if (!(Modules.Manipulator.networked = state))
                    boundNetwork = false;
            }));

            (Targeted = Body.Find(nameof(Targeted)).GetComponent<Toggle>()).onValueChanged.AddListener(new Action<bool>(state =>
            {
                if (Modules.Manipulator.targeted = state)
                {
                    if (!Networked.isOn)
                    {
                        Networked.Set(true);
                        boundNetwork = true;
                    }
                }
                else if (boundNetwork)
                    Networked.Set(false);
            }));

            AlphaText = Body.Find("Alpha/Text").GetComponent<Text>();
            BetaText = Body.Find("Beta/Text").GetComponent<Text>();
            GammaText = Body.Find("Gamma/Text").GetComponent<Text>();

            (Alpha = Body.Find("Alpha").GetComponent<Button>()).onClick.AddListener(new Action(() => Modules.Manipulator.SetRegister(ref Modules.Manipulator.regAlpha, AlphaText, nameof(Alpha))));
            (Beta =  Body.Find("Beta").GetComponent<Button>()).onClick.AddListener(new Action(() => Modules.Manipulator.SetRegister(ref Modules.Manipulator.regBeta, BetaText, nameof(Beta))));
            (Gamma = Body.Find("Gamma").GetComponent<Button>()).onClick.AddListener(new Action(() => Modules.Manipulator.SetRegister(ref Modules.Manipulator.regGamma, GammaText, nameof(Gamma))));

            (Broadcast = Body.Find(nameof(Broadcast)).GetComponent<Button>()).onClick.AddListener(new Action(() => Modules.Manipulator.Broadcast(BroadcastName.text)));
            BroadcastName = Body.Find(nameof(BroadcastName)).GetComponent<InputField>();

            Targets.Setup(Body.Find(nameof(Targets)));
            Events.Setup(Body.Find(nameof(Events)));
        }
        
        public static void ShowBehaviours()
        {
            Modules.Manipulator._cached = UnityEngine.Object.FindObjectsOfType<UdonBehaviour>();
            for (int i = 2; i < Targets.Template.parent.childCount; i++)
                Targets.Template.parent.GetChild(i).Destroy();
            int index = 0;
            foreach (UdonBehaviour behaviour in Modules.Manipulator._cached)
            {
                GameObject obj = Targets.Template.gameObject.Instantiate();
                obj.transform.SetParent(Targets.Template.parent);
                obj.transform.Find("Index/Text").GetComponent<Text>().text = (++index).ToString();
                obj.transform.Find("Text/Text").GetComponent<Text>().text = behaviour.name;
                obj.active = true;
                obj.GetComponent<Button>().onClick.AddListener(new Action(() => ShowEvents(Modules.Manipulator._current = behaviour)));
            }
        }

        public static void ShowEvents(UdonBehaviour behaviour)
        {
            for (int i = 2; i < Events.Template.parent.childCount; i++)
                Events.Template.parent.GetChild(i).Destroy();
            int index = 0;
            foreach (var kvp in behaviour._eventTable)
            {
                GameObject obj = Events.Template.gameObject.Instantiate();
                obj.transform.SetParent(Events.Template.parent);
                obj.transform.Find("Index/Text").GetComponent<Text>().text = (++index).ToString();
                obj.transform.Find("Text/Text").GetComponent<Text>().text = kvp.Key;
                obj.active = true;
                obj.GetComponent<Button>().onClick.AddListener(new Action(() => Modules.Manipulator.Send((behaviour, Modules.Manipulator._lastEvent = kvp.key))));
            }
        }

        public static class Targets
        {
            public static Transform Body;
            public static Transform Template;

            public static void Setup(Transform self)
            {
                Body = self.Find(nameof(Body));
                Common.Window.Create(self, self.Find("Title"), Body)
                    .Closable();

                Template = Body.Find("Scroll/Template");

                Body.Find("Refresh").GetComponent<Button>().onClick.AddListener((UnityAction)ShowBehaviours);
            }
        }

        public static class Events
        {
            public static Transform Body;
            public static Transform Template;

            public static void Setup(Transform self)
            {
                Body = self.Find(nameof(Body));
                Common.Window.Create(self, self.Find("Title"), Body)
                    .Closable();

                Template = Body.Find("Scroll/Template");
            }
        }
    }
}
