global using Events = KiraiMod.Events;
using BepInEx.IL2CPP;
using System;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace KiraiMod
{
    public static class Events
    {
        public static event Action ApplicationStart; // scene 0
        public static event Action UIManagerLoaded; // scene 1
        public static event Action<Scene> WorldLoaded; // scene -1
        public static event Action Update;

        static Events()
        {
            SceneManager.add_sceneLoaded((UnityAction<Scene, LoadSceneMode>)HookSceneLoaded);
            IL2CPPChainloader.AddUnityComponent<MonoHelper>();
        }

        public static void HookSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Shared.Logger.LogInfo($"Loading scene {scene.buildIndex}: {scene.name}");

            if (scene.buildIndex == -1)
                WorldLoaded?.Invoke(scene);
            else if (scene.buildIndex == 0)
                ApplicationStart?.Invoke();
            else if (scene.buildIndex == 1)
                UIManagerLoaded?.Invoke();
        }

        public class MonoHelper : MonoBehaviour
        {
            public MonoHelper() : base(ClassInjector.DerivedConstructorPointer<MonoHelper>()) => ClassInjector.DerivedConstructorBody(this);
            public MonoHelper(IntPtr ptr) : base(ptr) {}

            public void Update() => Events.Update?.Invoke();
        }
    }
}
