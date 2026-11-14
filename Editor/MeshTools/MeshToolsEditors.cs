using UnityEditor;
using UnityEngine;

namespace com.Plapamaru.Shared.MeshTools
{
    [CustomEditor(typeof(MeshCombiner))]
    public class MeshCombinerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.Space(10f);
            if (GUILayout.Button("Combine And Save"))
                Ping(((MeshCombiner)target).CombineAndSave());
        }

        public static void Ping(Object asset)
        {
            if (asset != null)
                EditorGUIUtility.PingObject(asset);
        }
    }

    [CustomEditor(typeof(MeshPivotChanger))]
    public class MeshPivotChangerEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            GUILayout.Space(10f);
            if (GUILayout.Button("Change Pivot And Save"))
                MeshCombinerEditor.Ping(((MeshPivotChanger)target).ChangePivotAndSave());
        }
    }
}
