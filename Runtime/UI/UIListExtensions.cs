using System.Collections.Generic;
using UnityEngine;

namespace com.Plapamaru.Shared.UI
{
    public static class UIListExtensions
    {
        public static void SetItemCount<T>(this IList<T> items, int count) where T : Component
        {
            if (items.Count == 0)
            {
                Debug.LogError($"{nameof(SetItemCount)} needs a template item at index 0.");
                return;
            }

            var template = items[0];
            var parent = template.transform.parent;

            while (items.Count < count)
            {
                var item = Object.Instantiate(template, parent);
                item.name = $"{template.name}{items.Count}";
                items.Add(item);
            }

            for (var i = 0; i < items.Count; i++)
                items[i].gameObject.SetActive(i < count);
        }
    }
}
