using System.IO;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Presets;
#endif

namespace com.Plapamaru.Shared.Screenshots
{
    public class ScreenshotTaker : MonoBehaviour
    {
        [SerializeField, FormerlySerializedAs("mainCamera")] private Camera _camera;
        [SerializeField, Tooltip("0 uses the screen width")] private int _width = 512;
        [SerializeField, Tooltip("0 uses the screen height")] private int _height = 512;
        [SerializeField] private bool _transparentBackground = true;
        [SerializeField] private Object _locationFolder;
        [SerializeField] private string _fileName;
        [SerializeField, FormerlySerializedAs("_importSettings"), Tooltip("Optional TextureImporter preset")] private Object _importPreset;

        public byte[] CapturePng()
        {
            Camera camera = _camera != null ? _camera : Camera.main;
            int width = _width > 0 ? _width : Screen.width;
            int height = _height > 0 ? _height : Screen.height;
            return ScreenshotUtility.CapturePng(camera, width, height, _transparentBackground);
        }

        public void SaveScreenshot(string absolutePath)
        {
            string directory = Path.GetDirectoryName(absolutePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);
            File.WriteAllBytes(absolutePath, CapturePng());
        }

#if ENABLE_LEGACY_INPUT_MANAGER && UNITY_EDITOR
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.T))
                SaveScreenshot();
        }
#endif

#if UNITY_EDITOR
        [ContextMenu("Take Screenshot")]
        public void SaveScreenshot()
        {
            string folder = _locationFolder != null ? AssetDatabase.GetAssetPath(_locationFolder) : null;
            if (string.IsNullOrEmpty(folder) || !AssetDatabase.IsValidFolder(folder) || string.IsNullOrWhiteSpace(_fileName))
            {
                Debug.LogError("Set a location folder and a file name first.", this);
                return;
            }
            SaveScreenshot(folder, _fileName, _importPreset as Preset);
        }

        public string SaveScreenshot(string assetFolderPath, string fileName, Preset importPreset)
        {
            string path = $"{assetFolderPath}/{fileName}.png";
            File.WriteAllBytes(path, CapturePng());
            AssetDatabase.ImportAsset(path);
            ApplyImportPreset(path, importPreset);
            return path;
        }

        private void ApplyImportPreset(string path, Preset preset)
        {
            if (preset == null || !(AssetImporter.GetAtPath(path) is TextureImporter importer))
                return;

            if (!preset.ApplyTo(importer))
            {
                Debug.LogWarning($"Could not apply import preset to {path}.", this);
                return;
            }
            importer.SaveAndReimport();
        }
#endif
    }
}
