using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KiraiMod.GUI.Common
{
    public static class Window
    {
        public static void Create(Transform Target)
        {
            Transform Title = Target.Find("Title");
            Transform Body = Target.Find("Body");

            EventTrigger trigger = Title.gameObject.AddComponent<EventTrigger>();
            trigger.triggers = new(2);
            trigger.triggers.Add(new EventTrigger.Entry().Setup(EventTriggerType.Drag, new Action<BaseEventData>(data => Target.position += (Vector3)data.Cast<PointerEventData>().delta)));
            trigger.triggers.Add(new EventTrigger.Entry().Setup(EventTriggerType.PointerClick, new Action<BaseEventData>(data => Body.gameObject.active ^= !data.Cast<PointerEventData>().dragging)));
        }
    }
}
