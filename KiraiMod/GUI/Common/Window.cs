using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KiraiMod.GUI.Common
{
    public class Window
    {
        EventTrigger Trigger;

        Transform Target;
        Transform Body;

        public static Window Create(Transform Target)
        {
            Transform Title = Target.Find("Title");
            Transform Body = Target.Find("Body");

            return Create(Target, Title, Body);
        }

        public static Window Create(Transform Target, Transform Title, Transform Body)
        {
            Window w = new();
            w.Target = Target;
            w.Body = Body;
            w.Trigger = Title.gameObject.AddComponent<EventTrigger>();
            w.Trigger.triggers = new();
            return w;
        }

        public Window Dragable()
        {
            Trigger.triggers.Add(new EventTrigger.Entry().Setup(EventTriggerType.Drag, new Action<BaseEventData>(data => Target.position += (Vector3)data.Cast<PointerEventData>().delta)));
            return this;
        }

        public Window Closable()
        {
            Trigger.triggers.Add(new EventTrigger.Entry().Setup(EventTriggerType.PointerClick, new Action<BaseEventData>(data => Body.gameObject.active ^= !data.Cast<PointerEventData>().dragging)));
            return this;
        }
    }
}
