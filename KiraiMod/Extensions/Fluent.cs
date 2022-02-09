using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace KiraiMod.Extensions
{
    public static class Fluent
    {
        public static GameObject Instantiate(this GameObject go) => Object.Instantiate(go);

        public static GameObject DontDestroyOnLoad(this GameObject go)
        {
            UnityEngine.Object.DontDestroyOnLoad(go);
            return go;
        }

        public static EventTrigger.Entry Setup(this EventTrigger.Entry entry, EventTriggerType type, UnityAction<BaseEventData> callback)
        {
            entry.eventID = type;
            entry.callback.AddListener(callback);
            return entry;
        }
    }
}
