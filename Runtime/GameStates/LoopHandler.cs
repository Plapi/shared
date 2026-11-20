using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace com.Plapamaru.Shared.GameStates
{
    public readonly struct LoopHandler
    {
        public readonly Func<CancellationToken, UniTask> Wait;
        public readonly Func<CancellationToken, UniTask<LoopResult>> Handle;

        public LoopHandler(Func<CancellationToken, UniTask> wait, Func<CancellationToken, UniTask<LoopResult>> handle)
        {
            Wait = wait;
            Handle = handle;
        }
    }
}
