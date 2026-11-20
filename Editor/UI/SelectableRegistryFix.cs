using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace com.Plapamaru.Shared.UI
{
    [InitializeOnLoad]
    internal static class SelectableRegistryFix
    {
        private const BindingFlags StaticFlags = BindingFlags.NonPublic | BindingFlags.Static;
        private const BindingFlags InstanceFlags = BindingFlags.NonPublic | BindingFlags.Instance;

        static SelectableRegistryFix()
        {
            EditorApplication.playModeStateChanged += state =>
            {
                if (state == PlayModeStateChange.EnteredPlayMode)
                    Rebuild();
            };
        }

        private static void Rebuild()
        {
            var type = typeof(Selectable);
            var list = type.GetField("s_Selectables", StaticFlags);
            var count = type.GetField("s_SelectableCount", StaticFlags);
            var enableCalled = type.GetField("m_EnableCalled", InstanceFlags);
            var index = type.GetField("m_CurrentIndex", InstanceFlags);
            if (list == null || count == null || enableCalled == null || index == null)
            {
                Debug.LogWarning($"{nameof(SelectableRegistryFix)}: uGUI Selectable internals changed, registry not rebuilt.");
                return;
            }

            var registered = new List<Selectable>();
            foreach (var selectable in Resources.FindObjectsOfTypeAll<Selectable>())
            {
                if ((bool)enableCalled.GetValue(selectable))
                    registered.Add(selectable);
            }

            var array = new Selectable[Mathf.Max(10, registered.Count * 2)];
            for (var i = 0; i < registered.Count; i++)
            {
                array[i] = registered[i];
                index.SetValue(registered[i], i);
            }

            list.SetValue(null, array);
            count.SetValue(null, registered.Count);
        }
    }
}
