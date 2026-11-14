#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace com.Plapamaru.Shared.MeshTools
{
    public static class MeshAssetUtility
    {
        public static bool TryGetFolderPath(Object folder, out string path)
        {
            path = folder != null ? AssetDatabase.GetAssetPath(folder) : null;
            if (!string.IsNullOrEmpty(path) && AssetDatabase.IsValidFolder(path))
                return true;

            Debug.LogError("Output folder is missing or is not a folder.");
            return false;
        }

        public static GameObject SaveAsPrefab(GameObject obj, string folderPath, string assetName)
        {
            if (string.IsNullOrWhiteSpace(assetName))
            {
                Debug.LogError("Output name is empty.");
                return null;
            }

            MeshFilter filter = obj.GetComponent<MeshFilter>();
            if (filter != null && filter.sharedMesh != null && !AssetDatabase.Contains(filter.sharedMesh))
            {
                string meshPath = $"{folderPath}/{assetName}.mesh";
                AssetDatabase.CreateAsset(filter.sharedMesh, meshPath);
                filter.sharedMesh = AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            }

            return PrefabUtility.SaveAsPrefabAsset(obj, $"{folderPath}/{assetName}.prefab");
        }
    }
}
#endif
