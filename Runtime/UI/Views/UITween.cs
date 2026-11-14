using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace com.Plapamaru.Shared.UI
{
    public static class UITween
    {
        public static async UniTask Run(float duration, Action<float> step, CancellationToken cancellationToken)
        {
            if (duration > 0f)
            {
                var elapsed = 0f;
                while (elapsed < duration)
                {
                    step(elapsed / duration);
                    await UniTask.Yield(PlayerLoopTiming.Update, cancellationToken);
                    elapsed += Time.unscaledDeltaTime;
                }
            }

            cancellationToken.ThrowIfCancellationRequested();
            step(1f);
        }
    }
}
