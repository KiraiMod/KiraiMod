using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VRC.SDKBase;
using VRC.Udon;
using VRC.Udon.Common.Interfaces;

namespace KiraiMod.Modules
{
    public static class Manipulator
    {
        private static bool repeater;
        public static bool Repeater
        {
            get => repeater;
            set
            {
                if (repeater == value) return;
                repeater = value;

                if (value)
                    Events.Update += Repeat;
                else Events.Update -= Repeat;
            }
        }

        public static bool networked;
        public static bool targeted;

        public static (UdonBehaviour, string) regAlpha;
        public static (UdonBehaviour, string) regBeta;
        public static (UdonBehaviour, string) regGamma;

        public static UdonBehaviour _current;
        public static string _lastEvent;
        public static UdonBehaviour[] _cached;

        static Manipulator()
        {
            Events.WorldLoaded += _ => GUI.Manipulator.ShowBehaviours();
        }

        public static void Send((UdonBehaviour behaviour, string eventName) reg)
        {
            if (targeted) { } // networked is assumed
            else if (networked)
                reg.behaviour.SendCustomNetworkEvent(NetworkEventTarget.All, reg.eventName);
            else reg.behaviour.SendCustomEvent(reg.eventName);
        }

        private static void Repeat()
        {
            if (regAlpha.Item1 != null) Send(regAlpha);
            if (regBeta.Item1 != null) Send(regBeta);
            if (regGamma.Item1 != null) Send(regGamma);
        }

        public static void SetRegister(ref (UdonBehaviour, string) register, Text display, string placeholder) 
        {
            if (register.Item1 != null)
            {
                display.text = placeholder;
                register = (null, null);
            }
            else
            {
                display.text = $"{_current.name}.{_lastEvent}";
                register = (_current, _lastEvent);
            }
        }

        public static void Broadcast(string eventName)
        {
            foreach (UdonBehaviour ub in _cached)
                foreach (var kvp in ub._eventTable)
                    if (kvp.key == eventName)
                        Send((ub, eventName));
        }
    }
}
