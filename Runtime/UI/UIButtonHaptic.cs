using com.Plapamaru.Shared.Haptics;
using UnityEngine;
using UnityEngine.UI;

namespace com.Plapamaru.Shared.UI
{
    [DisallowMultipleComponent]
    [AddComponentMenu("UI/Button Haptic")]
    public class UIButtonHaptic : MonoBehaviour
    {
        [SerializeField] private HapticFeedback.Type _type = HapticFeedback.Type.Light;

        private Button _button;
        private Toggle _toggle;

        private void Awake()
        {
            if (TryGetComponent(out _button))
                _button.onClick.AddListener(Vibrate);
            else if (TryGetComponent(out _toggle))
                _toggle.onValueChanged.AddListener(OnToggleChanged);
            else
                Debug.LogWarning($"{GetType().Name} needs a Button or a Toggle.", this);
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(Vibrate);
            if (_toggle != null)
                _toggle.onValueChanged.RemoveListener(OnToggleChanged);
        }

        private void OnToggleChanged(bool _)
        {
            Vibrate();
        }

        private void Vibrate()
        {
            HapticFeedback.VibrateHaptic(_type);
        }
    }
}
