using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace com.Plapamaru.Shared.UI
{
    public abstract class UITransition : MonoBehaviour
    {
        public const float DefaultDuration = 0.2f;

        [SerializeField] private float _duration = DefaultDuration;

        public float Duration
        {
            get => _duration;
            set => _duration = Mathf.Max(0f, value);
        }

        public abstract void SetShown();
        public abstract void SetHidden();
        public abstract UniTask Show(CancellationToken cancellationToken);
        public abstract UniTask Hide(CancellationToken cancellationToken);
    }
}
