global using KiraiMod.Extensions;

using System;

namespace KiraiMod.Extensions
{
    public static class Extensions
    {
        public static void Initialize(this Type type) => System.Runtime.CompilerServices.RuntimeHelpers.RunClassConstructor(type.TypeHandle);
    }
}
