using UnityEngine;

namespace com.Plapamaru.Shared.UI
{
    public class UIObject : MonoBehaviour
    {
        private RectTransform _rectTransform;

        public RectTransform RectTransform
        {
            get
            {
                if (_rectTransform == null)
                    _rectTransform = (RectTransform)transform;

                return _rectTransform;
            }
        }
    }
}
