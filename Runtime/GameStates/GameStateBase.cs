using System.Threading;
using com.Plapamaru.Shared.Async;
using Cysharp.Threading.Tasks;

namespace com.Plapamaru.Shared.GameStates
{
    public abstract class GameStateBase
    {
        public abstract class Context
        {
        }

        private UniTaskCompletionSource _backRequest;

        public GameStateMachine Machine { get; internal set; }

        protected virtual LoopHandler[] Handlers => null;

        protected virtual UniTask OnEnter(CancellationToken cancellationToken) => UniTask.CompletedTask;
        protected virtual UniTask OnExit(CancellationToken cancellationToken) => UniTask.CompletedTask;
        protected virtual UniTask OnPause(CancellationToken cancellationToken) => UniTask.CompletedTask;
        protected virtual UniTask OnResume(CancellationToken cancellationToken) => UniTask.CompletedTask;

        protected internal virtual bool OnBack()
        {
            var request = _backRequest;
            _backRequest = null;
            return request != null && request.TrySetResult();
        }

        protected UniTask WaitForBack(CancellationToken cancellationToken)
        {
            var request = new UniTaskCompletionSource();
            _backRequest = request;
            cancellationToken.Register(() =>
            {
                if (_backRequest == request)
                    _backRequest = null;
            });
            return request.Task.AttachExternalCancellation(cancellationToken);
        }

        internal abstract UniTask Run(CancellationToken cancellationToken);

        internal UniTask Enter(CancellationToken cancellationToken) => OnEnter(cancellationToken);
        internal UniTask Exit(CancellationToken cancellationToken) => OnExit(cancellationToken);
        internal UniTask Pause(CancellationToken cancellationToken) => OnPause(cancellationToken);
        internal UniTask Resume(CancellationToken cancellationToken) => OnResume(cancellationToken);

        protected static async UniTask RunLoop(CancellationToken cancellationToken, params LoopHandler[] handlers)
        {
            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var winner = await UniTaskRace.Race(handlers, (handler, token) => handler.Wait(token), cancellationToken);
                if (await winner.Handle(cancellationToken) == LoopResult.Break)
                    return;
            }
        }

        public override string ToString()
        {
            return GetType().Name;
        }
    }

    public abstract class GameStateBase<TContext> : GameStateBase where TContext : GameStateBase.Context
    {
        protected TContext _context;

        internal void SetContext(TContext context)
        {
            _context = context;
        }
    }
}
