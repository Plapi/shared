using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace com.Plapamaru.Shared.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Toggle))]
    [AddComponentMenu("UI/Switch")]
    public class UISwitch : MonoBehaviour
    {
        [SerializeField] private Image _background;
        [SerializeField] private Image _handle;
        [SerializeField] private Sprite _backgroundOn;
        [SerializeField] private Sprite _backgroundOff;
        [SerializeField] private Sprite _handleOn;
        [SerializeField] private Sprite _handleOff;
        [SerializeField] private float _handleOnX = 43f;
        [SerializeField] private float _handleOffX = -43f;
        [SerializeField, Min(0f)] private float _duration = 0.15f;
        [SerializeField] private UIEase _ease = UIEase.OutBack;

        private Toggle _toggle;
        private Coroutine _routine;
        private bool _shown;

        private Toggle Toggle
        {
            get
            {
                if (_toggle == null)
                    _toggle = GetComponent<Toggle>();

                return _toggle;
            }
        }

        private void Awake()
        {
            Toggle.onValueChanged.AddListener(OnValueChanged);
        }

        private void OnDestroy()
        {
            if (_toggle != null)
                _toggle.onValueChanged.RemoveListener(OnValueChanged);
        }

        private void OnEnable()
        {
            Snap(Toggle.isOn);
        }

        private void OnDisable()
        {
            _routine = null;
        }

        private void LateUpdate()
        {
            if (_routine == null && _shown != Toggle.isOn)
                Snap(Toggle.isOn);
        }

        private void OnValueChanged(bool isOn)
        {
            if (!isActiveAndEnabled || _duration <= 0f)
            {
                Snap(isOn);
                return;
            }

            if (_routine != null)
                StopCoroutine(_routine);
            _routine = StartCoroutine(Animate(isOn));
        }

        private IEnumerator Animate(bool isOn)
        {
            SetState(isOn);
            var from = _handle != null ? _handle.rectTransform.anchoredPosition.x : 0f;
            var to = isOn ? _handleOnX : _handleOffX;
            for (var elapsed = 0f; elapsed < _duration; elapsed += Time.unscaledDeltaTime)
            {
                SetHandleX(Mathf.LerpUnclamped(from, to, _ease.Evaluate(elapsed / _duration)));
                yield return null;
            }

            SetHandleX(to);
            _routine = null;
        }

        private void Snap(bool isOn)
        {
            if (_routine != null)
            {
                StopCoroutine(_routine);
                _routine = null;
            }

            SetState(isOn);
            SetHandleX(isOn ? _handleOnX : _handleOffX);
        }

        private void SetState(bool isOn)
        {
            _shown = isOn;
            if (_background != null)
                _background.sprite = isOn ? _backgroundOn : _backgroundOff;
            if (_handle != null)
                _handle.sprite = isOn ? _handleOn : _handleOff;
        }

        private void SetHandleX(float x)
        {
            if (_handle == null)
                return;

            var position = _handle.rectTransform.anchoredPosition;
            position.x = x;
            _handle.rectTransform.anchoredPosition = position;
        }
    }
}
