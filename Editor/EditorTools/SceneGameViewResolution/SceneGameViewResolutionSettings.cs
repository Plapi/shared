using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace com.Plapamaru.Shared.EditorTools
{
    [CreateAssetMenu(fileName = "SceneGameViewResolutionSettings", menuName = "Shared/Scene Game View Resolution Settings")]
    public class SceneGameViewResolutionSettings : ScriptableObject
    {
        [SerializeField] private List<SceneGameViewResolutionEntry> _sceneResolutions = new List<SceneGameViewResolutionEntry>();

        public IReadOnlyList<SceneGameViewResolutionEntry> SceneResolutions => _sceneResolutions;

        public void AddOrUpdateCurrentScene()
        {
            Scene scene = SceneManager.GetActiveScene();
            if (string.IsNullOrEmpty(scene.path))
            {
                Debug.LogWarning("Save the current scene before adding a Game view resolution entry.");
                return;
            }

            if (!SceneGameViewResolutionUtility.TryGetCurrentResolution(out SceneGameViewResolutionEntry resolution))
            {
                Debug.LogWarning("Could not read the current Game view resolution. Open the Game window and choose a resolution first.");
                return;
            }

            resolution.scene = AssetDatabase.LoadAssetAtPath<SceneAsset>(scene.path);
            resolution.sceneName = scene.name;
            resolution.scenePath = scene.path;

            int existing = IndexOf(scene);
            if (existing >= 0)
                _sceneResolutions[existing] = resolution;
            else
                _sceneResolutions.Add(resolution);

            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssetIfDirty(this);
        }

        public bool TryGetResolution(Scene scene, out SceneGameViewResolutionEntry resolution)
        {
            int index = IndexOf(scene);
            resolution = index >= 0 ? _sceneResolutions[index] : default;
            return index >= 0;
        }

        private int IndexOf(Scene scene)
        {
            for (int i = 0; i < _sceneResolutions.Count; i++)
            {
                SceneGameViewResolutionEntry entry = _sceneResolutions[i];
                string entryPath = entry.scene != null ? AssetDatabase.GetAssetPath(entry.scene) : entry.scenePath;
                if (entryPath == scene.path)
                    return i;
            }
            return -1;
        }
    }

    [Serializable]
    public struct SceneGameViewResolutionEntry
    {
        public SceneAsset scene;
        public string sceneName;
        public string scenePath;
        public int width;
        public int height;
        public string baseText;
        public int sizeType;
        public int selectedIndex;
    }

    [CustomEditor(typeof(SceneGameViewResolutionSettings))]
    public class SceneGameViewResolutionSettingsEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            if (GUILayout.Button("Add Current Scene"))
                ((SceneGameViewResolutionSettings)target).AddOrUpdateCurrentScene();
        }
    }
}
