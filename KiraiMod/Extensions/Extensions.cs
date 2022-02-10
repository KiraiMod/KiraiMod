global using KiraiMod.Extensions;

using System;
using UnityEngine;

namespace KiraiMod.Extensions
{
    public static class Extensions
    {
        public static void Initialize(this Type type) => System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);
        public static Managers.GUIManager.KiraiSlider GetKiraiSlider(this Transform obj) => new(obj);
    }
}
