using System;
using System.Collections;
using System.IO;
using com.Plapamaru.Shared.EditorTools;
using UnityEditor;
using UnityEngine;

namespace com.Plapamaru.Shared.Screenshots
{
    [CustomEditor(typeof(ScreenshotTaker))]
    public class ScreenshotTakerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            EditorGUILayout.Space();
            if (GUILayout.Button("Take Screenshot"))
                ((ScreenshotTaker)target).SaveScreenshot();
        }
    }

    public static class GameViewScreenshotMenu
    {
        private const double TimeoutSeconds = 10;

        [MenuItem("Tools/Shared/Take Game View Screenshot %k")]
        private static void TakeScreenshot()
        {
            EditorCoroutine.Start(TakeScreenshotRoutine());
        }

        private static IEnumerator TakeScreenshotRoutine()
        {
            string fileName = $"ScreenCapture {DateTime.Now:yyyy-MM-dd HH-mm-ss}.png";
            string tempPath = Path.GetFullPath(Path.Combine("Temp", fileName));
            ScreenCapture.CaptureScreenshot(tempPath);

            double deadline = EditorApplication.timeSinceStartup + TimeoutSeconds;
            while (!File.Exists(tempPath))
            {
                if (EditorApplication.timeSinceStartup > deadline)
                {
                    Debug.LogWarning("Screenshot timed out. Open the Game view so it can render.");
                    yield break;
                }
                EditorApplication.QueuePlayerLoopUpdate();
                yield return null;
            }
            yield return EditorCoroutine.WaitForSeconds(0.1);

            string desktopPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), fileName);
            File.Move(tempPath, desktopPath);
            EditorUtility.OpenWithDefaultApp(desktopPath);
        }
    }
}
