using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace KiraiMod.GUI.Common
{
    public class Window
    {
        private Transform Root;
        private EventTrigger Trigger;

        public static Window Create(Transform Root)
        {
            Window w = new();
            w.Root = Root;
            w.Trigger = Root.Find("Title").gameObject.AddComponent<EventTrigger>();
            w.Trigger.triggers = new();
            return w;
        }

        public Window Dragable()
        {
            Trigger.triggers.Add(new EventTrigger.Entry().Setup(EventTriggerType.Drag, new Action<BaseEventData>(data => Root.position += (Vector3)data.Cast<PointerEventData>().delta)));
            return this;
        }

        public Window Closable(GameObject body)
        {
            Trigger.triggers.Add(new EventTrigger.Entry().Setup(EventTriggerType.PointerClick, new Action<BaseEventData>(data => body.active ^= !data.Cast<PointerEventData>().dragging)));
            return this;
        }

        public Window Pinnable()
        {
            Root.Find("Title/Pin").GetComponent<Toggle>().On(state => Root.SetParent(state ? Managers.GUIManager.Pinned.transform : Managers.GUIManager.UserInterface.transform));
            return this;
        }
    }
}
