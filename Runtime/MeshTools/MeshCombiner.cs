using System;
using UnityEngine;
using UnityEngine.Serialization;
using Object = UnityEngine.Object;

namespace com.Plapamaru.Shared.MeshTools
{
    public class MeshCombiner : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("objFolder")] private Object _outputFolder;
        [SerializeField, FormerlySerializedAs("objName")] private string _outputName;
        [SerializeField] private bool _includeInactive = true;

        public void SetOutput(Object outputFolder, string outputName)
        {
            _outputFolder = outputFolder;
            _outputName = outputName;
        }

        public GameObject Combine(Transform root = null)
        {
            return MeshUtils.Combine(root != null ? root : transform, _includeInactive);
        }

#if UNITY_EDITOR
        public GameObject CombineAndSave(Action<GameObject> onCreated = null)
        {
            return CombineAndSave(transform, onCreated);
        }

        public GameObject CombineAndSave(Transform root, Action<GameObject> onCreated = null)
        {
            if (!MeshAssetUtility.TryGetFolderPath(_outputFolder, out string folderPath))
                return null;

            GameObject combined = Combine(root);
            try
            {
                combined.name = _outputName;
                combined.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                onCreated?.Invoke(combined);
                return MeshAssetUtility.SaveAsPrefab(combined, folderPath, _outputName);
            }
            finally
            {
                DestroyImmediate(combined);
            }
        }
#endif
    }
}
