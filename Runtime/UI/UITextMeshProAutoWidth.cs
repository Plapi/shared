#if SHARED_TMP
using TMPro;
using UnityEngine;

namespace com.Plapamaru.Shared.UI
{
    [ExecuteAlways]
    [RequireComponent(typeof(TextMeshProUGUI))]
    public class UITextMeshProAutoWidth : MonoBehaviour
    {
        [SerializeField] private float _padding;
        [SerializeField] private float _minWidth;
        [SerializeField] private float _maxWidth = -1f;

        private TextMeshProUGUI _text;
        private RectTransform _rectTransform;
        private bool _isUpdatingWidth;

        private TextMeshProUGUI Text
        {
            get
            {
                if (_text == null)
                    _text = GetComponent<TextMeshProUGUI>();

                return _text;
            }
        }

        private RectTransform CachedRectTransform
        {
            get
            {
                if (_rectTransform == null)
                    _rectTransform = (RectTransform)transform;

                return _rectTransform;
            }
        }

        private void OnEnable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Add(OnTextChanged);
            UpdateWidth();
        }

        private void OnDisable()
        {
            TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(OnTextChanged);
        }

        private void OnValidate()
        {
            _minWidth = Mathf.Max(0f, _minWidth);

            if (isActiveAndEnabled)
                UpdateWidth();
        }

        private void OnTextChanged(Object changedObject)
        {
            if (changedObject == Text)
                UpdateWidth();
        }

        public void UpdateWidth()
        {
            if (_isUpdatingWidth || Text == null)
                return;

            _isUpdatingWidth = true;

            try
            {
                Text.ForceMeshUpdate();

                var preferredWidth = Text.GetPreferredValues(Text.text, Mathf.Infinity, CachedRectTransform.rect.height).x;
                var width = Mathf.Max(_minWidth, preferredWidth + _padding);

                if (_maxWidth >= 0f)
                    width = Mathf.Min(width, _maxWidth);

                if (!Mathf.Approximately(CachedRectTransform.rect.width, width))
                    CachedRectTransform.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            }
            finally
            {
                _isUpdatingWidth = false;
            }
        }
    }
}
#endif
