using UnityEngine;

namespace com.Plapamaru.Shared.UI
{
    [CreateAssetMenu(menuName = "Shared/UI/Press Scale Settings", fileName = DefaultResourcePath)]
    public class UIPressScaleSettings : ScriptableObject
    {
        public const string DefaultResourcePath = "UIPressScaleDefault";

        [SerializeField] private float _pressedScale = 0.9f;
        [SerializeField] private float _pressDuration = 0.08f;
        [SerializeField] private UIEase _pressEase = UIEase.OutQuad;
        [SerializeField] private float _releaseDuration = 0.2f;
        [SerializeField] private UIEase _releaseEase = UIEase.OutBack;

        private static UIPressScaleSettings _default;

        public float PressedScale => _pressedScale;
        public float PressDuration => _pressDuration;
        public UIEase PressEase => _pressEase;
        public float ReleaseDuration => _releaseDuration;
        public UIEase ReleaseEase => _releaseEase;

        public static UIPressScaleSettings Default
        {
            get
            {
                if (_default == null)
                {
                    _default = Resources.Load<UIPressScaleSettings>(DefaultResourcePath);
                    if (_default == null)
                        _default = CreateInstance<UIPressScaleSettings>();
                }

                return _default;
            }
        }
    }
}
