namespace com.Plapamaru.Shared.UI
{
    public enum UIEase
    {
        Linear,
        InQuad,
        OutQuad,
        InOutQuad,
        InCubic,
        OutCubic,
        InBack,
        OutBack
    }

    public static class UIEaseExtensions
    {
        private const float BackOvershoot = 1.70158f;

        public static float Evaluate(this UIEase ease, float t)
        {
            switch (ease)
            {
                case UIEase.InQuad:
                    return t * t;
                case UIEase.OutQuad:
                    return 1f - (1f - t) * (1f - t);
                case UIEase.InOutQuad:
                    return t < 0.5f ? 2f * t * t : 1f - 2f * (1f - t) * (1f - t);
                case UIEase.InCubic:
                    return t * t * t;
                case UIEase.OutCubic:
                    return 1f - (1f - t) * (1f - t) * (1f - t);
                case UIEase.InBack:
                    return (BackOvershoot + 1f) * t * t * t - BackOvershoot * t * t;
                case UIEase.OutBack:
                    var u = t - 1f;
                    return 1f + (BackOvershoot + 1f) * u * u * u + BackOvershoot * u * u;
                default:
                    return t;
            }
        }
    }
}
