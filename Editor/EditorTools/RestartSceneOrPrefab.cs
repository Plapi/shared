using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace com.Plapamaru.Shared.EditorTools
{
    public static class RestartSceneOrPrefab
    {
        private const string MenuPath = "Tools/Shared/Restart Scene or Prefab %t";

        [MenuItem(MenuPath)]
        private static void RestartFromMenu()
        {
            Restart(true);
        }

        public static void Restart(bool askBeforeDiscarding)
        {
            if (EditorApplication.isPlaying)
            {
                ReloadActiveSceneInPlayMode();
                return;
            }

            var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
            if (prefabStage != null)
                ReloadPrefab(prefabStage, askBeforeDiscarding);
            else
                ReloadOpenScenes(askBeforeDiscarding);
        }

        private static void ReloadActiveSceneInPlayMode()
        {
            var scene = SceneManager.GetActiveScene();
            if (string.IsNullOrEmpty(scene.path))
            {
                Debug.LogWarning("Restart: the active scene has never been saved, so it can't be reloaded.");
                return;
            }

            EditorSceneManager.LoadSceneInPlayMode(scene.path, new LoadSceneParameters(LoadSceneMode.Single));
        }

        private static void ReloadPrefab(PrefabStage stage, bool askBeforeDiscarding)
        {
            var path = stage.assetPath;
            var name = System.IO.Path.GetFileNameWithoutExtension(path);

            if (stage.scene.isDirty && askBeforeDiscarding &&
                !EditorUtility.DisplayDialog("Restart prefab", $"Discard unsaved changes to {name} and reload it from disk?", "Discard", "Cancel"))
                return;

            var openedFromInstance = stage.openedFromInstanceRoot;
            var mode = stage.mode;

            stage.ClearDirtiness();
            StageUtility.GoToMainStage();

            if (openedFromInstance != null && mode == PrefabStage.Mode.InContext)
                PrefabStageUtility.OpenPrefab(path, openedFromInstance, mode);
            else
                PrefabStageUtility.OpenPrefab(path);
        }

        private static void ReloadOpenScenes(bool askBeforeDiscarding)
        {
            var activeScene = SceneManager.GetActiveScene();
            var scenes = new List<(string path, bool loaded)>();
            var anyDirty = false;

            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (string.IsNullOrEmpty(scene.path))
                {
                    Debug.LogWarning($"Restart: '{scene.name}' has never been saved, so it can't be reloaded.");
                    return;
                }

                scenes.Add((scene.path, scene.isLoaded));
                anyDirty |= scene.isDirty;
            }

            if (anyDirty && askBeforeDiscarding &&
                !EditorUtility.DisplayDialog("Restart scene", "Discard unsaved scene changes and reload from disk?", "Discard", "Cancel"))
                return;

            for (var i = 0; i < scenes.Count; i++)
            {
                var (path, loaded) = scenes[i];
                var mode = i == 0 ? OpenSceneMode.Single : loaded ? OpenSceneMode.Additive : OpenSceneMode.AdditiveWithoutLoading;
                EditorSceneManager.OpenScene(path, mode);
            }

            var reopenedActive = SceneManager.GetSceneByPath(activeScene.path);
            if (reopenedActive.IsValid() && reopenedActive.isLoaded)
                SceneManager.SetActiveScene(reopenedActive);
        }
    }
}
