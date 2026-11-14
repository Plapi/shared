using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.Compilation;
using UnityEngine;

namespace com.Plapamaru.Shared.EditorTools
{
    public static class RecompileProject
    {
        private const string MenuPath = "Tools/Shared/Recompile Project";
        private const string PendingKey = "com.Plapamaru.Shared.RecompileProject.Pending";
        private const double SettleSeconds = 1.0;

        private static readonly Type LogEntries = Type.GetType("UnityEditor.LogEntries, UnityEditor");

        private static bool _compilationFinished;
        private static double _idleSince;

        [MenuItem(MenuPath)]
        private static void RecompileFromMenu()
        {
            Recompile();
        }

        [MenuItem(MenuPath, true)]
        private static bool CanRecompile()
        {
            return !EditorApplication.isPlayingOrWillChangePlaymode && !EditorApplication.isCompiling;
        }

        public static void Recompile()
        {
            ClearConsole();
            SessionState.SetBool(PendingKey, true);

            _compilationFinished = false;
            CompilationPipeline.compilationFinished -= OnCompilationFinished;
            CompilationPipeline.compilationFinished += OnCompilationFinished;
            WaitForIdle();

            AssetDatabase.Refresh();
            CompilationPipeline.RequestScriptCompilation(RequestScriptCompilationOptions.CleanBuildCache);
        }

        [InitializeOnLoadMethod]
        private static void ResumeAfterDomainReload()
        {
            if (!SessionState.GetBool(PendingKey, false))
                return;

            _compilationFinished = true;
            WaitForIdle();
        }

        private static void OnCompilationFinished(object context)
        {
            CompilationPipeline.compilationFinished -= OnCompilationFinished;
            _compilationFinished = true;
        }

        private static void WaitForIdle()
        {
            _idleSince = EditorApplication.timeSinceStartup;
            EditorApplication.update -= UpdateWhileWaiting;
            EditorApplication.update += UpdateWhileWaiting;
        }

        private static void UpdateWhileWaiting()
        {
            if (!_compilationFinished || EditorApplication.isCompiling || EditorApplication.isUpdating)
            {
                _idleSince = EditorApplication.timeSinceStartup;
                return;
            }

            if (EditorApplication.timeSinceStartup - _idleSince < SettleSeconds)
                return;

            EditorApplication.update -= UpdateWhileWaiting;
            SessionState.EraseBool(PendingKey);
            LogSummary();
        }

        private static void LogSummary()
        {
            var counts = new object[] { 0, 0, 0 };
            var getCounts = LogEntries?.GetMethod("GetCountsByType", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (getCounts == null)
            {
                Debug.Log("Recompile Project: done (couldn't read the Console counts).");
                return;
            }

            getCounts.Invoke(null, counts);
            var errors = (int)counts[0];
            var warnings = (int)counts[1];
            var summary = $"Recompile Project: {errors} errors, {warnings} warnings.";

            if (errors > 0)
                Debug.LogError(summary);
            else if (warnings > 0)
                Debug.LogWarning(summary);
            else
                Debug.Log(summary);
        }

        private static void ClearConsole()
        {
            var clear = LogEntries?.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            if (clear != null)
                clear.Invoke(null, null);
            else
                Debug.LogWarning("Recompile Project: couldn't clear the Console (UnityEditor.LogEntries.Clear not found).");
        }
    }
}
