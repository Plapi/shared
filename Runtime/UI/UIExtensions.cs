using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace com.Plapamaru.Shared.UI
{
    public static class UIExtensions
    {
        public static void SetExclusiveListener(this Button button, UnityAction action)
        {
            button.onClick = new Button.ButtonClickedEvent();
            button.onClick.AddListener(action);
        }

        public static void SetAlpha(this Graphic graphic, float alpha)
        {
            Color color = graphic.color;
            color.a = alpha;
            graphic.color = color;
        }
    }
}
