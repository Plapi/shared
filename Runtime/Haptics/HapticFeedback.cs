using System;
using System.Runtime.InteropServices;
using UnityEngine;

namespace com.Plapamaru.Shared.Haptics
{
    public static class HapticFeedback
    {
        public enum Type
        {
            Heavy = 0,
            Light = 1,
            Medium = 2,
            Rigid = 3,
            Soft = 4
        }

        public static bool Enabled { get; private set; } = true;

        public static void SetEnabled(bool enabled)
        {
            Enabled = enabled;
        }

        public static void VibrateHaptic(Type type)
        {
            if (!Enabled)
                return;

#if UNITY_IOS && !UNITY_EDITOR
            PlapamaruHaptics_Impact((int)type);
#elif UNITY_ANDROID && !UNITY_EDITOR
            Android.Vibrate(type);
#endif
        }

#if UNITY_IOS && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern void PlapamaruHaptics_Impact(int type);
#endif

#if UNITY_ANDROID && !UNITY_EDITOR
        private static class Android
        {
            private const int EFFECT_CLICK = 0;
            private const int EFFECT_TICK = 2;
            private const int EFFECT_HEAVY_CLICK = 5;

            private static AndroidJavaObject _vibrator;
            private static AndroidJavaClass _vibrationEffect;
            private static int _sdk = -1;
            private static bool _unavailable;

            public static void Vibrate(Type type)
            {
                if (!EnsureVibrator())
                    return;

                try
                {
                    if (_sdk >= 29)
                    {
                        using var effect = _vibrationEffect.CallStatic<AndroidJavaObject>("createPredefined", PredefinedEffect(type));
                        _vibrator.Call("vibrate", effect);
                    }
                    else if (_sdk >= 26)
                    {
                        (long duration, int amplitude) = OneShot(type);
                        using var effect = _vibrationEffect.CallStatic<AndroidJavaObject>("createOneShot", duration, amplitude);
                        _vibrator.Call("vibrate", effect);
                    }
                    else
                    {
                        _vibrator.Call("vibrate", OneShot(type).duration);
                    }
                }
                catch (Exception e)
                {
                    _unavailable = true;
                    Debug.LogWarning($"Haptics disabled: {e.Message}");
                }
            }

            private static bool EnsureVibrator()
            {
                if (_unavailable)
                    return false;
                if (_vibrator != null)
                    return true;

                try
                {
                    using var version = new AndroidJavaClass("android.os.Build$VERSION");
                    _sdk = version.GetStatic<int>("SDK_INT");

                    using var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
                    using var activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                    _vibrator = activity.Call<AndroidJavaObject>("getSystemService", "vibrator");

                    if (_vibrator == null || !_vibrator.Call<bool>("hasVibrator"))
                    {
                        _unavailable = true;
                        return false;
                    }

                    if (_sdk >= 26)
                        _vibrationEffect = new AndroidJavaClass("android.os.VibrationEffect");
                    return true;
                }
                catch (Exception e)
                {
                    _unavailable = true;
                    Debug.LogWarning($"Haptics disabled: {e.Message}");
                    return false;
                }
            }

            private static int PredefinedEffect(Type type) => type switch
            {
                Type.Heavy => EFFECT_HEAVY_CLICK,
                Type.Medium => EFFECT_CLICK,
                Type.Rigid => EFFECT_CLICK,
                _ => EFFECT_TICK
            };

            private static (long duration, int amplitude) OneShot(Type type) => type switch
            {
                Type.Heavy => (40L, 255),
                Type.Medium => (25L, 180),
                Type.Rigid => (15L, 220),
                _ => (10L, 100)
            };
        }
#endif
    }
}
