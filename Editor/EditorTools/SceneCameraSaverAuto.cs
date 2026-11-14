using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace com.Plapamaru.Shared.EditorTools
{
    [InitializeOnLoad]
    public static class SceneCameraSaverAuto
    {
        private const string KeyPrefix = "Plapamaru.SceneCamera.";
        private const string EnabledKey = KeyPrefix + "Enabled";
        private const string MenuPath = "Tools/Shared/Remember Scene Camera";

        private static State _last;

        [Serializable]
        private struct State
        {
            public Vector3 pivot;
            public Quaternion rotation;
            public float size;
            public bool orthographic;
            public bool in2DMode;
        }

        static SceneCameraSaverAuto()
        {
            SceneView.duringSceneGui += OnSceneGUI;
            EditorSceneManager.sceneOpened += OnSceneOpened;
        }

        private static bool Enabled
        {
            get => EditorPrefs.GetBool(EnabledKey, true);
            set => EditorPrefs.SetBool(EnabledKey, value);
        }

        [MenuItem(MenuPath)]
        private static void ToggleEnabled() => Enabled = !Enabled;

        [MenuItem(MenuPath, true)]
        private static bool ToggleEnabledValidate()
        {
            Menu.SetChecked(MenuPath, Enabled);
            return true;
        }

        private static void OnSceneGUI(SceneView sceneView)
        {
            if (!Enabled || sceneView != SceneView.lastActiveSceneView || Event.current.type != EventType.Repaint)
                return;

            string key = KeyFor(SceneManager.GetActiveScene());
            if (key == null)
                return;

            var state = new State
            {
                pivot = sceneView.pivot,
                rotation = sceneView.rotation,
                size = sceneView.size,
                orthographic = sceneView.orthographic,
                in2DMode = sceneView.in2DMode
            };
            if (state.Equals(_last))
                return;

            _last = state;
            EditorPrefs.SetString(key, JsonUtility.ToJson(state));
        }

        private static void OnSceneOpened(Scene scene, OpenSceneMode mode)
        {
            if (!Enabled || mode != OpenSceneMode.Single)
                return;

            SceneView sceneView = SceneView.lastActiveSceneView;
            string key = KeyFor(scene);
            if (sceneView == null || key == null || !EditorPrefs.HasKey(key))
                return;

            State state = JsonUtility.FromJson<State>(EditorPrefs.GetString(key));
            _last = state;
            sceneView.in2DMode = state.in2DMode;
            sceneView.orthographic = state.orthographic;
            if (state.in2DMode)
                sceneView.LookAtDirect(state.pivot, sceneView.rotation, state.size);
            else
                sceneView.LookAtDirect(state.pivot, state.rotation, state.size);
            sceneView.Repaint();
        }

        private static string KeyFor(Scene scene)
        {
            if (string.IsNullOrEmpty(scene.path))
                return null;
            string guid = AssetDatabase.AssetPathToGUID(scene.path);
            return string.IsNullOrEmpty(guid) ? null : KeyPrefix + guid;
        }
    }
}
