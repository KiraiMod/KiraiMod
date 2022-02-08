using System;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.EventSystems;

namespace KiraiMod.GUI.Behaviours
{
    public class Dragable : MonoBehaviour
    {
        public Transform target;

        public Dragable() : base(ClassInjector.DerivedConstructorPointer<Dragable>()) => ClassInjector.DerivedConstructorBody(this);
        public Dragable(IntPtr ptr) : base(ptr) {}

        public void Start()
        {
            target ??= transform;

            EventTrigger.Entry entry = new();
            entry.eventID = EventTriggerType.Drag;
            entry.callback.AddListener((UnityEngine.Events.UnityAction<BaseEventData>)OnDrag);

            EventTrigger trigger = gameObject.AddComponent<EventTrigger>();
            trigger.triggers = new(1);
            trigger.triggers.Add(entry);
        }

        private void OnDrag(BaseEventData data) => target.position = data.Cast<PointerEventData>().position;
    }
}
