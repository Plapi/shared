using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace com.Plapamaru.Shared.UI
{
    public class UISlideTransition : UITransition
    {
        [SerializeField] private RectTransform _target;
        [SerializeField] private Vector2 _hiddenOffset = new Vector2(0f, -800f);
        [SerializeField] private UIEase _showEase = UIEase.OutCubic;
        [SerializeField] private UIEase _hideEase = UIEase.InCubic;

        private bool _captured;
        private Vector2 _shownPosition;

        private RectTransform Target
        {
            get
            {
                if (_target == null)
                    _target = (RectTransform)transform;

                return _target;
            }
        }

        public override void SetShown()
        {
            Capture();
            Target.anchoredPosition = _shownPosition;
        }

        public override void SetHidden()
        {
            Capture();
            Target.anchoredPosition = _shownPosition + _hiddenOffset;
        }

        public override UniTask Show(CancellationToken cancellationToken)
        {
            Capture();
            return Slide(_shownPosition, _showEase, cancellationToken);
        }

        public override UniTask Hide(CancellationToken cancellationToken)
        {
            Capture();
            return Slide(_shownPosition + _hiddenOffset, _hideEase, cancellationToken);
        }

        private UniTask Slide(Vector2 to, UIEase ease, CancellationToken cancellationToken)
        {
            var target = Target;
            var from = target.anchoredPosition;
            return UITween.Run(Duration, t => target.anchoredPosition = Vector2.LerpUnclamped(from, to, ease.Evaluate(t)), cancellationToken);
        }

        private void Capture()
        {
            if (_captured)
                return;

            _captured = true;
            _shownPosition = Target.anchoredPosition;
        }
    }
}
