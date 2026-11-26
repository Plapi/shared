using UnityEngine;
using UnityEngine.UI;

namespace com.Plapamaru.Shared.UI
{
    [DisallowMultipleComponent]
    [AddComponentMenu("UI/Fill Bar")]
    public class UIFillBar : MonoBehaviour
    {
        public enum FillAxis
        {
            Horizontal,
            Vertical
        }

        [Tooltip("Resized along the axis (anchors 0..value), so a sliced sprite keeps its ends.")]
        [SerializeField] private RectTransform _fill;
        [Tooltip("Optional. Sits behind the fill and shows the amount just lost, then catches up after Trail Delay.")]
        [SerializeField] private RectTransform _trail;
        [Tooltip("Optional. Faded in as the value drops (alpha = 1 - value), e.g. a red fill over a green one.")]
        [SerializeField] private Graphic _lowOverlay;
        [Tooltip("Optional. Punched in scale whenever the value drops.")]
        [SerializeField] private RectTransform _punchTarget;
        [SerializeField] private FillAxis _axis;
        [SerializeField, Range(0f, 1f)] private float _value = 1f;
        [SerializeField, Min(0f)] private float _fillDuration = 0.25f;
        [SerializeField] private UIEase _fillEase = UIEase.OutCubic;
        [SerializeField, Min(0f)] private float _trailDelay = 0.4f;
        [SerializeField, Min(0f)] private float _trailDuration = 0.35f;
        [SerializeField] private UIEase _trailEase = UIEase.OutQuad;
        [SerializeField] private float _punchScale = 0.15f;
        [SerializeField, Min(0f)] private float _punchDuration = 0.25f;

        private float _shown;
        private float _fillFrom;
        private float _fillElapsed = float.MaxValue;
        private float _trailShown;
        private float _trailFrom;
        private float _trailElapsed = float.MaxValue;
        private float _punchElapsed = float.MaxValue;

        public float Value => _value;

        public FillAxis Axis
        {
            get => _axis;
            set
            {
                _axis = value;
                Apply();
            }
        }

        private void OnEnable()
        {
            Snap();
        }

        public void SetValue(float value, bool animate = true)
        {
            value = Mathf.Clamp01(value);
            if (!animate || !isActiveAndEnabled)
            {
                _value = value;
                Snap();
                return;
            }

            if (value < _value)
            {
                _trailShown = Mathf.Max(_trailShown, _shown);
                _trailFrom = _trailShown;
                _trailElapsed = -_trailDelay;
                _punchElapsed = 0f;
            }
            else
            {
                _trailShown = value;
                _trailElapsed = float.MaxValue;
            }

            _fillFrom = _shown;
            _fillElapsed = 0f;
            _value = value;
        }

        private void Update()
        {
            float dt = Time.unscaledDeltaTime;
            bool changed = false;

            if (_fillElapsed < _fillDuration)
            {
                _fillElapsed += dt;
                _shown = Mathf.LerpUnclamped(_fillFrom, _value, _fillEase.Evaluate(Mathf.Clamp01(_fillElapsed / _fillDuration)));
                changed = true;
            }
            else if (!Mathf.Approximately(_shown, _value))
            {
                _shown = _value;
                changed = true;
            }

            if (_trailElapsed < _trailDuration)
            {
                _trailElapsed += dt;
                if (_trailElapsed > 0f)
                    _trailShown = Mathf.LerpUnclamped(_trailFrom, _value, _trailEase.Evaluate(Mathf.Clamp01(_trailElapsed / Mathf.Max(_trailDuration, 0.0001f))));
                changed = true;
            }

            if (_punchElapsed < _punchDuration)
            {
                _punchElapsed += dt;
                changed = true;
            }

            if (changed)
                Apply();
        }

        private void Snap()
        {
            _shown = _trailShown = _value;
            _fillElapsed = _trailElapsed = _punchElapsed = float.MaxValue;
            Apply();
        }

        private void Apply()
        {
            SetAmount(_fill, _shown);
            SetAmount(_trail, Mathf.Max(_trailShown, _shown));

            if (_lowOverlay != null)
            {
                var color = _lowOverlay.color;
                color.a = 1f - _shown;
                _lowOverlay.color = color;
            }

            if (_punchTarget != null)
            {
                float p = _punchDuration > 0f ? Mathf.Clamp01(_punchElapsed / _punchDuration) : 1f;
                float scale = 1f + _punchScale * Mathf.Sin(p * Mathf.PI) * (1f - p);
                _punchTarget.localScale = new Vector3(scale, scale, 1f);
            }
        }

        private void SetAmount(RectTransform rect, float amount)
        {
            if (rect == null)
                return;

            bool visible = amount > 0.0001f;
            if (rect.gameObject.activeSelf != visible)
                rect.gameObject.SetActive(visible);

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = _axis == FillAxis.Horizontal ? new Vector2(amount, 1f) : new Vector2(1f, amount);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (this != null && !UnityEditor.EditorUtility.IsPersistent(this))
                    Snap();
            };
        }
#endif
    }
}
