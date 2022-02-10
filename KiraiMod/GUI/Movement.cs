using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static KiraiMod.Managers.GUIManager;

namespace KiraiMod.GUI
{
    public static class Movement
    {
        public static Toggle flight;
        public static Toggle directional;
        public static Toggle noclip;

        public static KiraiSlider flightSpeed;

        public static Transform Root;
        public static Transform Body;

        public static void Setup(Transform self)
        {
            Root = self;
            Body = self.Find(nameof(Body));

            EventTrigger trigger = self.Find("Title").gameObject.AddComponent<EventTrigger>();
            trigger.triggers = new(2);
            trigger.triggers.Add(new EventTrigger.Entry().Setup(EventTriggerType.Drag, (UnityAction<BaseEventData>)OnDrag));
            trigger.triggers.Add(new EventTrigger.Entry().Setup(EventTriggerType.PointerClick, (UnityAction<BaseEventData>)OnClick));

            (flight = Body.Find("Flight").GetComponent<Toggle>()).onValueChanged.AddListener(new Action<bool>(state => Modules.Flight.State = state));
            (directional = Body.Find("Directional").GetComponent<Toggle>()).onValueChanged.AddListener(new Action<bool>(state => Modules.Flight.directional.Value = state));
            (noclip = Body.Find("NoClip").GetComponent<Toggle>()).onValueChanged.AddListener(new Action<bool>(state => Modules.Flight.noclip.Value = state));
            (flightSpeed = Body.Find("FlightSpeed").GetKiraiSlider()).slider.onValueChanged.AddListener(new Action<float>(value => Modules.Flight.speed.Value = value));

            Modules.Flight.directional.GUIBind(directional);
            Modules.Flight.noclip.GUIBind(noclip);
            Modules.Flight.speed.GUIBind(flightSpeed);
        }

        private static void OnDrag(BaseEventData data) => Root.position += (Vector3)data.Cast<PointerEventData>().delta;
        private static void OnClick(BaseEventData data) => Body.gameObject.active ^= !data.Cast<PointerEventData>().dragging;
    }
}
