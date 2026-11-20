#if UNITY_ANDROID
using System.IO;
using System.Text;
using System.Xml;
using UnityEditor.Android;

namespace com.Plapamaru.Shared.Haptics
{
    internal class AndroidVibratePermission : IPostGenerateGradleAndroidProject
    {
        private const string AndroidNamespace = "http://schemas.android.com/apk/res/android";
        private const string Permission = "android.permission.VIBRATE";

        public int callbackOrder => 0;

        public void OnPostGenerateGradleAndroidProject(string path)
        {
            var manifestPath = Path.Combine(path, "src", "main", "AndroidManifest.xml");
            if (!File.Exists(manifestPath))
                return;

            var document = new XmlDocument();
            document.Load(manifestPath);
            var manifest = document.DocumentElement;
            if (manifest == null)
                return;

            foreach (XmlNode node in manifest.SelectNodes("uses-permission"))
            {
                if (node is XmlElement element && element.GetAttribute("name", AndroidNamespace) == Permission)
                    return;
            }

            var permission = document.CreateElement("uses-permission");
            permission.SetAttribute("name", AndroidNamespace, Permission);
            manifest.PrependChild(permission);
            using var writer = new StreamWriter(manifestPath, false, new UTF8Encoding(false));
            document.Save(writer);
        }
    }
}
#endif
