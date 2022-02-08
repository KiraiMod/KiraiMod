using UnityEngine;

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
    }
}
