using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace com.Plapamaru.Shared.UI
{
    public class UIFadeTransition : UITransition
    {
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private UIEase _showEase = UIEase.OutQuad;
        [SerializeField] private UIEase _hideEase = UIEase.InQuad;

        private CanvasGroup Group
        {
            get
            {
                if (_canvasGroup == null && !TryGetComponent(out _canvasGroup))
                    _canvasGroup = gameObject.AddComponent<CanvasGroup>();

                return _canvasGroup;
            }
        }

        public override void SetShown()
        {
            Group.alpha = 1f;
        }

        public override void SetHidden()
        {
            Group.alpha = 0f;
        }

        public override UniTask Show(CancellationToken cancellationToken)
        {
            return Fade(1f, _showEase, cancellationToken);
        }

        public override UniTask Hide(CancellationToken cancellationToken)
        {
            return Fade(0f, _hideEase, cancellationToken);
        }

        private UniTask Fade(float to, UIEase ease, CancellationToken cancellationToken)
        {
            var group = Group;
            var from = group.alpha;
            return UITween.Run(Duration, t => group.alpha = Mathf.LerpUnclamped(from, to, ease.Evaluate(t)), cancellationToken);
        }
    }
}
