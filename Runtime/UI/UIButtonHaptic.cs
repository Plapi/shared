using com.Plapamaru.Shared.Haptics;
using UnityEngine;
using UnityEngine.UI;

namespace com.Plapamaru.Shared.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    [AddComponentMenu("UI/Button Haptic")]
    public class UIButtonHaptic : MonoBehaviour
    {
        [SerializeField] private HapticFeedback.Type _type = HapticFeedback.Type.Light;

        private Button _button;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(Vibrate);
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(Vibrate);
        }

        private void Vibrate()
        {
            HapticFeedback.VibrateHaptic(_type);
        }
    }
}
