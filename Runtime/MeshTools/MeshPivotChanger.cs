using UnityEngine;
using UnityEngine.Serialization;

namespace com.Plapamaru.Shared.MeshTools
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class MeshPivotChanger : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("pivot")] private Vector3 _pivot = new Vector3(0.5f, 0f, 0.5f);
        [SerializeField, FormerlySerializedAs("originalMesh")] private Mesh _sourceMesh;
        [SerializeField, FormerlySerializedAs("objFolder")] private Object _outputFolder;
        [SerializeField, FormerlySerializedAs("objName")] private string _outputName;

        public GameObject CreateWithPivot()
        {
            Mesh source = _sourceMesh != null ? _sourceMesh : GetComponent<MeshFilter>().sharedMesh;
            Mesh mesh = MeshUtils.WithPivot(source, _pivot, out Vector3 offset);

            var result = new GameObject(gameObject.name + "Pivot");
            result.transform.SetPositionAndRotation(transform.TransformPoint(offset), transform.rotation);
            result.transform.localScale = transform.lossyScale;
            result.AddComponent<MeshFilter>().sharedMesh = mesh;
            result.AddComponent<MeshRenderer>().sharedMaterials = GetComponent<MeshRenderer>().sharedMaterials;
            return result;
        }

        private void Reset()
        {
            _sourceMesh = GetComponent<MeshFilter>().sharedMesh;
        }

#if UNITY_EDITOR
        [ContextMenu("Change Pivot And Save")]
        public GameObject ChangePivotAndSave()
        {
            if (!MeshAssetUtility.TryGetFolderPath(_outputFolder, out string folderPath))
                return null;

            GameObject result = CreateWithPivot();
            try
            {
                result.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
                return MeshAssetUtility.SaveAsPrefab(result, folderPath, _outputName);
            }
            finally
            {
                DestroyImmediate(result);
            }
        }
#endif
    }
}
