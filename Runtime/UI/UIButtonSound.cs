using com.Plapamaru.Shared.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace com.Plapamaru.Shared.UI
{
    [DisallowMultipleComponent]
    [AddComponentMenu("UI/Button Sound")]
    public class UIButtonSound : MonoBehaviour
    {
        [SerializeField] private AudioClip _clip;

        private Button _button;
        private Toggle _toggle;

        private void Awake()
        {
            if (TryGetComponent(out _button))
                _button.onClick.AddListener(Play);
            else if (TryGetComponent(out _toggle))
                _toggle.onValueChanged.AddListener(OnToggleChanged);
            else
                Debug.LogWarning($"{GetType().Name} needs a Button or a Toggle.", this);
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(Play);
            if (_toggle != null)
                _toggle.onValueChanged.RemoveListener(OnToggleChanged);
        }

        private void OnToggleChanged(bool _)
        {
            Play();
        }

        private void Play()
        {
            if (_clip != null && AudioManager.HasInstance())
                AudioManager.Instance.PlayEffect(_clip);
        }
    }
}
