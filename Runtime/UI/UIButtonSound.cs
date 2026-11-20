using com.Plapamaru.Shared.Audio;
using UnityEngine;
using UnityEngine.UI;

namespace com.Plapamaru.Shared.UI
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    [AddComponentMenu("UI/Button Sound")]
    public class UIButtonSound : MonoBehaviour
    {
        [SerializeField] private UIButtonSoundSettings _settings;

        private Button _button;

        private UIButtonSoundSettings Settings => _settings != null ? _settings : UIButtonSoundSettings.Default;

        private void Awake()
        {
            _button = GetComponent<Button>();
            _button.onClick.AddListener(Play);
        }

        private void OnDestroy()
        {
            if (_button != null)
                _button.onClick.RemoveListener(Play);
        }

        private void Play()
        {
            if (!AudioManager.HasInstance())
                return;

            var settings = Settings;
            AudioManager.Instance.PlayEffect(settings.Clip, settings.Volume);
        }
    }
}
