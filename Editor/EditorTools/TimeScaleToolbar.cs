using System;
using System.Globalization;
using UnityEditor;
using UnityEditor.Toolbars;
using UnityEngine;

namespace com.Plapamaru.Shared.EditorTools
{
    [InitializeOnLoad]
    public static class TimeScaleToolbar
    {
        private const string ValuePath = "Time Scale/Value";
        private const string SliderPath = "Time Scale/Slider";
        private const string PreferenceKey = "Shared.TimeScaleToolbar.Preference";
        private const string EditModeTimeScaleKey = "Shared.TimeScaleToolbar.EditModeTimeScale";
        private const float SliderMax = 10f;
        private const float Max = 100f;

        private static readonly float[] Presets = { 0f, 0.1f, 0.25f, 0.5f, 1f, 2f, 5f, 10f };

        private static float _shown;

        static TimeScaleToolbar()
        {
            _shown = Current;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
            EditorApplication.update += RefreshIfChangedExternally;
            if (!EditorPrefs.GetBool(ShownOnceKey))
                EditorApplication.update += ShowOnce;
        }

        private static string ShownOnceKey => "Shared.TimeScaleToolbar.ShownOnce." + Application.dataPath.GetHashCode();

        private static void ShowOnce()
        {
            var toolbar = Array.Find(Resources.FindObjectsOfTypeAll<EditorWindow>(), w => w.GetType().Name == "MainToolbarWindow");
            if (toolbar == null)
                return;

            EditorApplication.update -= ShowOnce;
            foreach (var path in new[] { ValuePath, SliderPath })
            {
                if (toolbar.TryGetOverlay(path, out var overlay))
                    overlay.displayed = true;
            }

            EditorPrefs.SetBool(ShownOnceKey, true);
        }

        public static float Current => EditorApplication.isPlaying ? Time.timeScale : Preference;

        private static float Preference
        {
            get => SessionState.GetFloat(PreferenceKey, 1f);
            set => SessionState.SetFloat(PreferenceKey, value);
        }

        public static void Set(float value)
        {
            value = Mathf.Clamp(Mathf.Round(value * 100f) / 100f, 0f, Max);
            Preference = value;
            if (EditorApplication.isPlaying)
                Time.timeScale = value;

            _shown = value;
            MainToolbar.Refresh(ValuePath);
        }

        [MainToolbarElement(ValuePath, defaultDockPosition = MainToolbarDockPosition.Middle, defaultDockIndex = 100)]
        private static MainToolbarElement CreateValue()
        {
            return new MainToolbarDropdown(new MainToolbarContent($"Time ×{Format(Current)}", "Time scale presets"), ShowPresets);
        }

        [MainToolbarElement(SliderPath, defaultDockPosition = MainToolbarDockPosition.Middle, defaultDockIndex = 101)]
        private static MainToolbarElement CreateSlider()
        {
            return new MainToolbarSlider(new MainToolbarContent(string.Empty, "Time scale"), Mathf.Min(Current, SliderMax), 0f, SliderMax, Set);
        }

        private static void ShowPresets(Rect rect)
        {
            var current = Current;
            var menu = new GenericMenu();
            foreach (var preset in Presets)
            {
                var value = preset;
                menu.AddItem(new GUIContent($"×{Format(value)}"), Mathf.Approximately(current, value), () => SetAndRefreshAll(value));
            }

            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Custom..."), false, () => CustomTimeScaleWindow.Open(current));
            menu.DropDown(rect);
        }

        private static void SetAndRefreshAll(float value)
        {
            Set(value);
            MainToolbar.Refresh(SliderPath);
        }

        private static void RefreshIfChangedExternally()
        {
            var current = Current;
            if (Mathf.Approximately(current, _shown))
                return;

            _shown = current;
            MainToolbar.Refresh(ValuePath);
            MainToolbar.Refresh(SliderPath);
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange change)
        {
            switch (change)
            {
                case PlayModeStateChange.ExitingEditMode:
                    SessionState.SetFloat(EditModeTimeScaleKey, Time.timeScale);
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    Time.timeScale = Preference;
                    break;
                case PlayModeStateChange.EnteredEditMode:
                    Time.timeScale = SessionState.GetFloat(EditModeTimeScaleKey, 1f);
                    break;
            }
        }

        private static string Format(float value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }

        private class CustomTimeScaleWindow : EditorWindow
        {
            private const string FieldName = "TimeScaleField";

            private string _text;
            private bool _focused;

            public static void Open(float value)
            {
                var window = CreateInstance<CustomTimeScaleWindow>();
                window._text = Format(value);
                window.titleContent = new GUIContent("Time Scale");

                var size = new Vector2(200f, 60f);
                var main = EditorGUIUtility.GetMainWindowPosition();
                window.position = new Rect(main.center - size / 2f, size);
                window.ShowUtility();
            }

            private void OnGUI()
            {
                var e = Event.current;
                if (e.type == EventType.KeyDown && e.keyCode == KeyCode.Escape)
                {
                    e.Use();
                    Close();
                    return;
                }

                var enterPressed = e.type == EventType.KeyDown && (e.keyCode == KeyCode.Return || e.keyCode == KeyCode.KeypadEnter);
                if (enterPressed)
                    e.Use();

                GUI.SetNextControlName(FieldName);
                _text = EditorGUILayout.TextField(_text);
                if (!_focused)
                {
                    EditorGUI.FocusTextInControl(FieldName);
                    _focused = true;
                }

                if (!GUILayout.Button("Apply") && !enterPressed)
                    return;

                if (float.TryParse(_text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
                    SetAndRefreshAll(value);

                Close();
            }
        }
    }
}
