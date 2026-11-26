using UnityEngine;

namespace com.Plapamaru.Shared.Utilities
{
    public static class ApplicationUtils
    {
        public static void Quit()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
}
