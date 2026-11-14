using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace com.Plapamaru.Shared.EditorTools
{
    [InitializeOnLoad]
    public static class SceneGameViewResolutionSaverAuto
    {
        private static bool _isRestoring;

        static SceneGameViewResolutionSaverAuto()
        {
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorApplication.delayCall += () => RestoreResolution(SceneManager.GetActiveScene());
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            if (mode == OpenSceneMode.Single)
                EditorApplication.delayCall += () => RestoreResolution(scene);
        }

        private static void RestoreResolution(Scene scene)
        {
            if (_isRestoring || !scene.IsValid())
                return;

            SceneGameViewResolutionSettings settings = FindSettings();
            if (settings == null || !settings.TryGetResolution(scene, out SceneGameViewResolutionEntry resolution))
                return;

            _isRestoring = true;
            try
            {
                SceneGameViewResolutionUtility.ApplyResolution(resolution);
            }
            finally
            {
                EditorApplication.delayCall += () => _isRestoring = false;
            }
        }

        private static SceneGameViewResolutionSettings FindSettings()
        {
            string[] guids = AssetDatabase.FindAssets($"t:{nameof(SceneGameViewResolutionSettings)}");
            return guids.Length == 0
                ? null
                : AssetDatabase.LoadAssetAtPath<SceneGameViewResolutionSettings>(AssetDatabase.GUIDToAssetPath(guids[0]));
        }
    }
}
