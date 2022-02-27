using UnityEngine;

namespace KiraiMod
{
    public static class Extensions
    {
        public static Managers.GUIManager.KiraiSlider GetKiraiSlider(this Transform obj) => new(obj);
    }
}
