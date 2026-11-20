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
        [SerializeField] private AudioClip _clip;

        private Button _button;

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
            if (_clip != null && AudioManager.HasInstance())
                AudioManager.Instance.PlayEffect(_clip);
        }
    }
}
