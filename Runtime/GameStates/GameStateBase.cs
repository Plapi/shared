using System;
using System.Collections.Generic;
using System.Threading;
using com.Plapamaru.Shared.Async;
using Cysharp.Threading.Tasks;

namespace com.Plapamaru.Shared.GameStates
{
    public abstract class GameStateBase
    {
        public GameStateMachine Machine { get; internal set; }

        protected virtual UniTask OnEnter(CancellationToken cancellationToken) => UniTask.CompletedTask;
        protected virtual UniTask OnExit(CancellationToken cancellationToken) => UniTask.CompletedTask;
        protected virtual UniTask OnPause(CancellationToken cancellationToken) => UniTask.CompletedTask;
        protected virtual UniTask OnResume(CancellationToken cancellationToken) => UniTask.CompletedTask;
        protected internal virtual bool OnBack() => false;

        internal abstract UniTask Run(CancellationToken cancellationToken);

        internal UniTask Enter(CancellationToken cancellationToken) => OnEnter(cancellationToken);
        internal UniTask Exit(CancellationToken cancellationToken) => OnExit(cancellationToken);
        internal UniTask Pause(CancellationToken cancellationToken) => OnPause(cancellationToken);
        internal UniTask Resume(CancellationToken cancellationToken) => OnResume(cancellationToken);

        protected static async UniTask RunLoop(
            CancellationToken cancellationToken,
            params (Func<CancellationToken, UniTask> wait, Func<CancellationToken, UniTask<LoopResult>> handle)[] handlers)
        {
            var race = new KeyValuePair<Func<CancellationToken, UniTask>, Func<CancellationToken, UniTask<LoopResult>>>[handlers.Length];
            for (var i = 0; i < handlers.Length; i++)
                race[i] = new KeyValuePair<Func<CancellationToken, UniTask>, Func<CancellationToken, UniTask<LoopResult>>>(handlers[i].wait, handlers[i].handle);

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var handle = await UniTaskRace.Race(race, cancellationToken);
                if (await handle(cancellationToken) == LoopResult.Break)
                    return;
            }
        }

        public override string ToString()
        {
            return GetType().Name;
        }
    }
}
