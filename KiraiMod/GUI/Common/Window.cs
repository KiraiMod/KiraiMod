using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KiraiMod.GUI.Common
{
    public static class Window
    {
        public static void Create(Transform Target, bool movable = true)
        {
            Transform Title = Target.Find("Title");
            Transform Body = Target.Find("Body");

            Create(Target, Title, Body, movable);
        }

        public static void Create(Transform Target, Transform Title, Transform Body, bool movable = true)
        {
            EventTrigger trigger = Title.gameObject.AddComponent<EventTrigger>();
            trigger.triggers = new(movable ? 2 : 1);
            if (movable) 
                trigger.triggers.Add(new EventTrigger.Entry().Setup(EventTriggerType.Drag, new Action<BaseEventData>(data => Target.position += (Vector3)data.Cast<PointerEventData>().delta)));
            trigger.triggers.Add(new EventTrigger.Entry().Setup(EventTriggerType.PointerClick, new Action<BaseEventData>(data => Body.gameObject.active ^= !data.Cast<PointerEventData>().dragging)));
        }
    }
}
