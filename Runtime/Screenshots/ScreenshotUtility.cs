using UnityEngine;

namespace com.Plapamaru.Shared.Screenshots
{
    public static class ScreenshotUtility
    {
        public static Texture2D Capture(Camera camera, int width, int height, bool transparentBackground = true)
        {
            RenderTexture previousTarget = camera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;
            CameraClearFlags previousClearFlags = camera.clearFlags;
            Color previousBackground = camera.backgroundColor;

            RenderTexture rt = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
            try
            {
                if (transparentBackground)
                {
                    camera.clearFlags = CameraClearFlags.SolidColor;
                    camera.backgroundColor = Color.clear;
                }

                camera.targetTexture = rt;
                camera.Render();

                RenderTexture.active = rt;
                var texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
                texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                texture.Apply();
                return texture;
            }
            finally
            {
                camera.targetTexture = previousTarget;
                camera.clearFlags = previousClearFlags;
                camera.backgroundColor = previousBackground;
                RenderTexture.active = previousActive;
                RenderTexture.ReleaseTemporary(rt);
            }
        }

        public static byte[] CapturePng(Camera camera, int width, int height, bool transparentBackground = true)
        {
            Texture2D texture = Capture(camera, width, height, transparentBackground);
            try
            {
                return texture.EncodeToPNG();
            }
            finally
            {
                if (Application.isPlaying)
                    Object.Destroy(texture);
                else
                    Object.DestroyImmediate(texture);
            }
        }
    }
}
