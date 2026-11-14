using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace com.Plapamaru.Shared.UI
{
    public class UIScaleTransition : UITransition
    {
        [SerializeField] private Graphic _background;
        [SerializeField] private CanvasGroup _content;
        [SerializeField] private float _hiddenScale = 0.8f;
        [SerializeField] private UIEase _showEase = UIEase.OutBack;
        [SerializeField] private UIEase _hideEase = UIEase.InQuad;

        private bool _captured;
        private float _backgroundAlpha = 1f;

        public override void SetShown()
        {
            Apply(1f, 1f, 1f);
        }

        public override void SetHidden()
        {
            Apply(0f, 0f, _hiddenScale);
        }

        public override UniTask Show(CancellationToken cancellationToken)
        {
            return Animate(1f, _showEase, cancellationToken);
        }

        public override UniTask Hide(CancellationToken cancellationToken)
        {
            return Animate(0f, _hideEase, cancellationToken);
        }

        private UniTask Animate(float visibility, UIEase ease, CancellationToken cancellationToken)
        {
            Capture();

            var backgroundFrom = _background != null && _backgroundAlpha > 0f ? _background.color.a / _backgroundAlpha : visibility;
            var alphaFrom = _content != null ? _content.alpha : visibility;
            var scaleFrom = _content != null ? _content.transform.localScale.x : 1f;
            var scaleTo = Mathf.LerpUnclamped(_hiddenScale, 1f, visibility);

            return UITween.Run(Duration, t =>
            {
                Apply(
                    Mathf.Lerp(backgroundFrom, visibility, t),
                    Mathf.Lerp(alphaFrom, visibility, t),
                    Mathf.LerpUnclamped(scaleFrom, scaleTo, ease.Evaluate(t)));
            }, cancellationToken);
        }

        private void Apply(float background, float alpha, float scale)
        {
            Capture();

            if (_background != null)
                _background.SetAlpha(background * _backgroundAlpha);

            if (_content != null)
            {
                _content.alpha = alpha;
                _content.transform.localScale = new Vector3(scale, scale, 1f);
            }
        }

        private void Capture()
        {
            if (_captured)
                return;

            _captured = true;
            if (_background != null)
                _backgroundAlpha = _background.color.a;
        }
    }
}
