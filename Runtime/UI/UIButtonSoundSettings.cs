using UnityEngine;

namespace com.Plapamaru.Shared.UI
{
    [CreateAssetMenu(menuName = "Shared/UI/Button Sound Settings", fileName = DefaultResourcePath)]
    public class UIButtonSoundSettings : ScriptableObject
    {
        public const string DefaultResourcePath = "UIButtonSoundDefault";

        [SerializeField] private AudioClip _clip;
        [SerializeField, Range(0f, 1f)] private float _volume = 1f;

        private static UIButtonSoundSettings _default;

        public AudioClip Clip => _clip;
        public float Volume => _volume;

        public static UIButtonSoundSettings Default
        {
            get
            {
                if (_default == null)
                {
                    _default = Resources.Load<UIButtonSoundSettings>(DefaultResourcePath);
                    if (_default == null)
                        _default = CreateInstance<UIButtonSoundSettings>();
                }

                return _default;
            }
        }
    }
}
