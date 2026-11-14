using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace com.Plapamaru.Shared.GameStates
{
    public sealed class GameStateMachine : IDisposable
    {
        private sealed class Entry
        {
            public readonly GameStateBase State;
            public readonly CancellationTokenSource Cts;

            public Entry(GameStateBase state, CancellationToken parentToken)
            {
                State = state;
                Cts = CancellationTokenSource.CreateLinkedTokenSource(parentToken);
            }
        }

        private readonly CancellationTokenSource _lifetime = new CancellationTokenSource();
        private readonly List<Entry> _stack = new List<Entry>();
        private readonly List<GameStateBase> _states = new List<GameStateBase>();

        private Entry _root;
        private GameStateBase _nextRoot;
        private bool _runningRoots;
        private bool _disposed;

        public IReadOnlyList<GameStateBase> States => _states;
        public GameStateBase Current => _states.Count > 0 ? _states[^1] : null;

        public event Action StateChanged;

        public void Change(GameStateBase state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            if (_disposed)
                throw new ObjectDisposedException(nameof(GameStateMachine));

            _nextRoot = state;
            _root?.Cts.Cancel();

            if (!_runningRoots)
                RunRoots().Forget();
        }

        public async UniTask Push(GameState state, CancellationToken cancellationToken = default)
        {
            await PushState(state, cancellationToken);
        }

        public async UniTask<TResult> Push<TResult>(GameState<TResult> state, CancellationToken cancellationToken = default)
        {
            await PushState(state, cancellationToken);
            return state.Result;
        }

        public bool HandleBack()
        {
            var current = Current;
            return current != null && current.OnBack();
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            _disposed = true;
            _nextRoot = null;
            _lifetime.Cancel();
        }

        private async UniTaskVoid RunRoots()
        {
            _runningRoots = true;
            try
            {
                while (_nextRoot != null && !_lifetime.IsCancellationRequested)
                {
                    if (_stack.Count > 0)
                    {
                        await UniTask.WaitUntil(() => _stack.Count == 0, cancellationToken: _lifetime.Token)
                            .SuppressCancellationThrow();
                        continue;
                    }

                    var root = _nextRoot;
                    _nextRoot = null;
                    await RunState(root, _lifetime.Token, true);
                }
            }
            finally
            {
                _runningRoots = false;
            }
        }

        private async UniTask PushState(GameStateBase state, CancellationToken cancellationToken)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            if (_stack.Count == 0)
                throw new InvalidOperationException($"No running state to push {state} onto. Use {nameof(Change)} to start the first state.");

            var parent = _stack[^1];
            var parentToken = parent.Cts.Token;
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(parentToken, cancellationToken);
            var token = linked.Token;

            await parent.State.Pause(parentToken);
            try
            {
                await RunState(state, token, false);
            }
            finally
            {
                if (!parentToken.IsCancellationRequested)
                    await parent.State.Resume(parentToken);
            }

            token.ThrowIfCancellationRequested();
        }

        private async UniTask RunState(GameStateBase state, CancellationToken parentToken, bool isRoot)
        {
            if (state.Machine != null)
                throw new InvalidOperationException($"{state} is already running.");

            var entry = new Entry(state, parentToken);
            var token = entry.Cts.Token;
            state.Machine = this;
            _stack.Add(entry);
            _states.Add(state);
            if (isRoot)
                _root = entry;
            StateChanged?.Invoke();

            try
            {
                await state.Enter(token);
                await state.Run(token);
            }
            catch (OperationCanceledException) when (token.IsCancellationRequested)
            {
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
                entry.Cts.Cancel();

                if (!_disposed)
                {
                    try
                    {
                        await state.Exit(_lifetime.Token);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                    catch (Exception exception)
                    {
                        Debug.LogException(exception);
                    }
                }

                _stack.Remove(entry);
                _states.Remove(state);
                if (_root == entry)
                    _root = null;
                entry.Cts.Dispose();
                state.Machine = null;
                StateChanged?.Invoke();
            }
        }
    }
}
