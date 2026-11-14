using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace com.Plapamaru.Shared.EditorTools
{
    public static class SceneGameViewResolutionUtility
    {
        private const BindingFlags AllFlags = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly Assembly EditorAssembly = typeof(EditorWindow).Assembly;
        private static readonly Type GameViewType = EditorAssembly.GetType("UnityEditor.GameView");
        private static readonly Type GameViewSizesType = EditorAssembly.GetType("UnityEditor.GameViewSizes");
        private static readonly Type GameViewSizeType = EditorAssembly.GetType("UnityEditor.GameViewSize");
        private static readonly Type GameViewSizeGroupType = EditorAssembly.GetType("UnityEditor.GameViewSizeGroup");
        private static readonly Type GameViewSizeKindType = EditorAssembly.GetType("UnityEditor.GameViewSizeType");

        public static bool TryGetCurrentResolution(out SceneGameViewResolutionEntry resolution)
        {
            resolution = default;
            try
            {
                EditorWindow gameView = GetGameView();
                object size = gameView == null ? null : GameViewType.GetProperty("currentGameViewSize", AllFlags)?.GetValue(gameView);
                if (size == null)
                    return false;

                resolution = new SceneGameViewResolutionEntry
                {
                    width = GetProperty<int>(size, "width"),
                    height = GetProperty<int>(size, "height"),
                    baseText = GetProperty<string>(size, "baseText"),
                    sizeType = GetSizeType(size),
                    selectedIndex = GetProperty<int>(gameView, "selectedSizeIndex")
                };
                return true;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Reading the Game view resolution failed (Unity internals changed?): {e.Message}");
                return false;
            }
        }

        public static void ApplyResolution(SceneGameViewResolutionEntry resolution)
        {
            try
            {
                EditorWindow gameView = GetGameView();
                object group = GetCurrentSizeGroup();
                if (gameView == null || group == null)
                    return;

                int index = FindResolutionIndex(group, resolution);
                if (index < 0 && resolution.width > 0 && resolution.height > 0)
                    index = AddCustomResolution(group, resolution);
                if (index < 0)
                    return;

                SetSelectedSizeIndex(gameView, index);
                SetScaleToMinimum(gameView);
                gameView.Repaint();
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Applying the Game view resolution failed (Unity internals changed?): {e.Message}");
            }
        }

        private static EditorWindow GetGameView()
        {
            if (GameViewType == null)
                return null;
            UnityEngine.Object[] gameViews = Resources.FindObjectsOfTypeAll(GameViewType);
            return gameViews.Length > 0 ? gameViews[0] as EditorWindow : null;
        }

        private static object GetGameViewSizesInstance()
        {
            return GameViewSizesType?.BaseType?.GetProperty("instance", AllFlags)?.GetValue(null);
        }

        private static object GetCurrentSizeGroup()
        {
            object sizes = GetGameViewSizesInstance();
            object group = GameViewSizesType?.GetProperty("currentGroup", AllFlags)?.GetValue(sizes);
            if (group != null)
                return group;

            object groupType = GameViewType?.GetProperty("currentSizeGroupType", AllFlags)?.GetValue(null);
            MethodInfo getGroup = GameViewSizesType?.GetMethod("GetGroup", AllFlags);
            return groupType == null ? null : getGroup?.Invoke(sizes, new[] { groupType });
        }

        private static int FindResolutionIndex(object group, SceneGameViewResolutionEntry resolution)
        {
            int total = (int)GameViewSizeGroupType.GetMethod("GetTotalCount", AllFlags).Invoke(group, null);
            MethodInfo getSize = GameViewSizeGroupType.GetMethod("GetGameViewSize", AllFlags);
            for (int i = 0; i < total; i++)
            {
                object size = getSize.Invoke(group, new object[] { i });
                if (size != null &&
                    GetProperty<int>(size, "width") == resolution.width &&
                    GetProperty<int>(size, "height") == resolution.height &&
                    GetSizeType(size) == resolution.sizeType)
                    return i;
            }
            return -1;
        }

        private static int AddCustomResolution(object group, SceneGameViewResolutionEntry resolution)
        {
            object fixedResolution = Enum.ToObject(GameViewSizeKindType, 1);
            string label = string.IsNullOrEmpty(resolution.baseText)
                ? $"Scene Resolution {resolution.width}x{resolution.height}"
                : resolution.baseText;
            object size = Activator.CreateInstance(GameViewSizeType, fixedResolution, resolution.width, resolution.height, label);

            GameViewSizeGroupType.GetMethod("AddCustomSize", AllFlags).Invoke(group, new[] { size });
            GameViewSizesType.GetMethod("SaveToHDD", AllFlags)?.Invoke(GetGameViewSizesInstance(), null);
            return FindResolutionIndex(group, resolution);
        }

        private static void SetSelectedSizeIndex(EditorWindow gameView, int index)
        {
            PropertyInfo selected = GameViewType.GetProperty("selectedSizeIndex", AllFlags);
            if (selected?.CanWrite == true)
            {
                selected.SetValue(gameView, index);
                return;
            }
            GameViewType.GetMethod("SizeSelectionCallback", AllFlags)?.Invoke(gameView, new object[] { index, null });
        }

        private static void SetScaleToMinimum(EditorWindow gameView)
        {
            MethodInfo snapZoom = GameViewType.GetMethod("SnapZoom", AllFlags);
            if (snapZoom == null)
                return;

            snapZoom.Invoke(gameView, new object[] { GetMinimumScale(gameView) });
            EditorApplication.delayCall += () =>
            {
                if (gameView == null)
                    return;
                snapZoom.Invoke(gameView, new object[] { GetMinimumScale(gameView) });
                gameView.Repaint();
            };
        }

        private static float GetMinimumScale(EditorWindow gameView)
        {
            PropertyInfo minScale = GameViewType.GetProperty("minScale", AllFlags);
            return minScale == null ? 0f : (float)minScale.GetValue(gameView);
        }

        private static T GetProperty<T>(object target, string name)
        {
            PropertyInfo property = target.GetType().GetProperty(name, AllFlags)
                ?? throw new MissingMemberException(target.GetType().Name, name);
            return (T)property.GetValue(target);
        }

        private static int GetSizeType(object size)
        {
            return Convert.ToInt32(GetProperty<object>(size, "sizeType"));
        }
    }
}
