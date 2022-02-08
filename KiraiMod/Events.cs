global using Events = KiraiMod.Events;

using System;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace KiraiMod
{
    public static class Events
    {
        public static Action ApplicationStart; // scene 0
        public static Action UIManagerLoaded; // scene 1
        public static Action<Scene> WorldLoaded; // scene -1

        static Events()
        {
            SceneManager.add_sceneLoaded((UnityAction<Scene, LoadSceneMode>)HookSceneLoaded);
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
    }
}
