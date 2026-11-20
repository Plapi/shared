using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace com.Plapamaru.Shared.UI
{
    [DisallowMultipleComponent]
    [AddComponentMenu("UI/Press Scale")]
    public class UIPressScale : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private UIPressScaleSettings _settings;
        [SerializeField] private Transform _target;

        private Selectable _selectable;
        private Vector3 _baseScale;
        private bool _captured;
        private int _pointerId;
        private bool _pressed;
        private float _factor = 1f;
        private Coroutine _animation;

        private Transform Target => _target != null ? _target : transform;
        private UIPressScaleSettings Settings => _settings != null ? _settings : UIPressScaleSettings.Default;

        private void Awake()
        {
            TryGetComponent(out _selectable);
            Capture();
        }

        private void OnDisable()
        {
            _pressed = false;
            _animation = null;
            Apply(1f);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (_pressed || eventData.button != PointerEventData.InputButton.Left)
                return;
            if (_selectable != null && !_selectable.IsInteractable())
                return;

            _pressed = true;
            _pointerId = eventData.pointerId;
            var settings = Settings;
            Animate(settings.PressedScale, settings.PressDuration, settings.PressEase);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_pressed || eventData.pointerId != _pointerId)
                return;

            _pressed = false;
            var settings = Settings;
            Animate(1f, settings.ReleaseDuration, settings.ReleaseEase);
        }

        private void Animate(float to, float duration, UIEase ease)
        {
            if (_animation != null)
                StopCoroutine(_animation);
            _animation = StartCoroutine(Run(_factor, to, duration, ease));
        }

        private IEnumerator Run(float from, float to, float duration, UIEase ease)
        {
            for (var elapsed = 0f; elapsed < duration; elapsed += Time.unscaledDeltaTime)
            {
                Apply(Mathf.LerpUnclamped(from, to, ease.Evaluate(elapsed / duration)));
                yield return null;
            }

            Apply(to);
            _animation = null;
        }

        private void Apply(float factor)
        {
            Capture();
            _factor = factor;
            Target.localScale = _baseScale * factor;
        }

        private void Capture()
        {
            if (_captured)
                return;

            _captured = true;
            _baseScale = Target.localScale;
        }
    }
}
