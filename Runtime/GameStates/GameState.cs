using System;
using System.Threading;
using Cysharp.Threading.Tasks;

namespace com.Plapamaru.Shared.GameStates
{
    public abstract class GameState : GameStateBase
    {
        protected virtual UniTask OnRun(CancellationToken cancellationToken)
        {
            var handlers = Handlers;
            if (handlers == null)
                throw new InvalidOperationException($"{this} must override {nameof(OnRun)} or {nameof(Handlers)}.");

            return RunLoop(cancellationToken, handlers);
        }

        internal override UniTask Run(CancellationToken cancellationToken)
        {
            return OnRun(cancellationToken);
        }
    }

    public abstract class GameState<TResult> : GameStateBase
    {
        public TResult Result { get; private set; }

        protected abstract UniTask<TResult> OnRun(CancellationToken cancellationToken);

        internal override async UniTask Run(CancellationToken cancellationToken)
        {
            Result = default;
            Result = await OnRun(cancellationToken);
        }
    }
}
