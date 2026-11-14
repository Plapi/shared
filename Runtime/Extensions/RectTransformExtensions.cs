using UnityEngine;

namespace com.Plapamaru.Shared.Extensions
{
    public static class RectTransformExtensions
    {
        public static void SetAnchorPosX(this RectTransform rectTransform, float x)
        {
            Vector2 p = rectTransform.anchoredPosition;
            p.x = x;
            rectTransform.anchoredPosition = p;
        }

        public static void SetAnchorPosY(this RectTransform rectTransform, float y)
        {
            Vector2 p = rectTransform.anchoredPosition;
            p.y = y;
            rectTransform.anchoredPosition = p;
        }
    }
}
