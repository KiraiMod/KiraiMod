using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KiraiMod.GUI
{
    public static class Movement
    {
        public static Transform Self;
        public static Transform Title;
        public static Transform Body;

        public static void Setup(Transform self)
        {
            Self = self;
            Title = self.Find(nameof(Title));
            Body = self.Find(nameof(Body));

            EventTrigger trigger = Title.gameObject.AddComponent<EventTrigger>();
            trigger.triggers = new(2);
            trigger.triggers.Add(new EventTrigger.Entry().Setup(EventTriggerType.Drag, (UnityAction<BaseEventData>)OnDrag));
            trigger.triggers.Add(new EventTrigger.Entry().Setup(EventTriggerType.PointerClick, (UnityAction<BaseEventData>)OnClick));

            Body.Find("Flight").GetComponent<Toggle>().onValueChanged.AddListener(new Action<bool>(state => Modules.Flight.State = state));
        }

        private static void OnDrag(BaseEventData data) => Self.position += (Vector3)data.Cast<PointerEventData>().delta;
        private static void OnClick(BaseEventData data) => Body.gameObject.active ^= !data.Cast<PointerEventData>().dragging;
    }
}
