using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace com.Plapamaru.Shared.UI
{
    public abstract class UIView : UIObject
    {
        [SerializeField] private UITransition _transition;
        [SerializeField] private Button[] _closeButtons = Array.Empty<Button>();
        [SerializeField] private bool _useViewStack;
        [SerializeField] private bool _startVisible;

        private UIManager _manager;
        private CancellationTokenSource _transitionCts;
        private UniTask _pending = UniTask.CompletedTask;

        public UIViewState State { get; private set; } = UIViewState.Hidden;
        public bool IsVisible => State == UIViewState.Showing || State == UIViewState.Shown;
        public bool UsesViewStack => _useViewStack;

        public event Action Shown;
        public event Action Hidden;

        public UniTask Show(CancellationToken cancellationToken = default)
        {
            return Show(true, cancellationToken);
        }

        public UniTask Show(bool animate, CancellationToken cancellationToken = default)
        {
            if (State == UIViewState.Shown)
                return UniTask.CompletedTask;

            if (State != UIViewState.Showing || !animate)
                _pending = Share(RunShow(animate, RestartTransition()));

            return _pending.AttachExternalCancellation(cancellationToken);
        }

        public UniTask Hide(CancellationToken cancellationToken = default)
        {
            return Hide(true, cancellationToken);
        }

        public UniTask Hide(bool animate, CancellationToken cancellationToken = default)
        {
            if (State == UIViewState.Hidden)
                return UniTask.CompletedTask;

            if (State != UIViewState.Hiding || !animate)
                _pending = Share(RunHide(animate, RestartTransition()));

            return _pending.AttachExternalCancellation(cancellationToken);
        }

        public UniTask WaitUntilHidden(CancellationToken cancellationToken = default)
        {
            return UniTask.WaitUntil(() => State == UIViewState.Hidden, cancellationToken: cancellationToken);
        }

        protected virtual void OnShow() { }
        protected virtual void OnShown() { }
        protected virtual void OnHide() { }
        protected virtual void OnHidden() { }

        protected virtual void OnCloseButton()
        {
            Hide().Forget();
        }

        protected internal virtual void OnBack()
        {
            Hide().Forget();
        }

        protected virtual void OnDestroy()
        {
            CancelTransition();

            if (_manager != null)
                _manager.Unregister(this);
        }

        internal void Attach(UIManager manager)
        {
            _manager = manager;

            if (_transition == null)
                TryGetComponent(out _transition);

            foreach (var button in _closeButtons)
            {
                if (button == null)
                    continue;

                button.onClick.RemoveListener(OnCloseButton);
                button.onClick.AddListener(OnCloseButton);
            }

            CancelTransition();
            _pending = UniTask.CompletedTask;

            if (_startVisible)
            {
                State = UIViewState.Shown;
                if (_transition != null)
                    _transition.SetShown();
                gameObject.SetActive(true);
                if (_useViewStack)
                    _manager.PushToStack(this);
            }
            else
            {
                State = UIViewState.Hidden;
                if (_transition != null)
                    _transition.SetHidden();
                gameObject.SetActive(false);
            }
        }

        internal void Detach()
        {
            _manager = null;
        }

        private async UniTask RunShow(bool animate, CancellationToken token)
        {
            var wasHidden = State == UIViewState.Hidden;
            var wasHiding = wasHidden || State == UIViewState.Hiding;

            State = UIViewState.Showing;

            if (_useViewStack && _manager != null)
                _manager.PushToStack(this);

            if (wasHidden && _transition != null)
                _transition.SetHidden();

            gameObject.SetActive(true);

            if (wasHiding)
                OnShow();

            if (animate && _transition != null)
            {
                if (await _transition.Show(token).SuppressCancellationThrow())
                    return;
            }
            else if (_transition != null)
            {
                _transition.SetShown();
            }

            State = UIViewState.Shown;
            OnShown();
            Shown?.Invoke();
        }

        private async UniTask RunHide(bool animate, CancellationToken token)
        {
            var wasShowing = State == UIViewState.Shown || State == UIViewState.Showing;

            State = UIViewState.Hiding;

            if (_manager != null)
                _manager.RemoveFromStack(this);

            if (wasShowing)
                OnHide();

            if (animate && _transition != null && gameObject.activeInHierarchy)
            {
                if (await _transition.Hide(token).SuppressCancellationThrow())
                    return;
            }

            gameObject.SetActive(false);
            State = UIViewState.Hidden;
            OnHidden();
            Hidden?.Invoke();
        }

        private static UniTask Share(UniTask task)
        {
            var source = new UniTaskCompletionSource();
            Complete(task, source).Forget();
            return source.Task;
        }

        private static async UniTaskVoid Complete(UniTask task, UniTaskCompletionSource source)
        {
            try
            {
                await task;
                source.TrySetResult();
            }
            catch (OperationCanceledException exception)
            {
                source.TrySetCanceled(exception.CancellationToken);
            }
            catch (Exception exception)
            {
                source.TrySetException(exception);
            }
        }

        private CancellationToken RestartTransition()
        {
            CancelTransition();
            _transitionCts = CancellationTokenSource.CreateLinkedTokenSource(destroyCancellationToken);
            return _transitionCts.Token;
        }

        private void CancelTransition()
        {
            if (_transitionCts == null)
                return;

            _transitionCts.Cancel();
            _transitionCts.Dispose();
            _transitionCts = null;
        }
    }

    public abstract class UIView<TData> : UIView
    {
        protected TData Model { get; private set; }

        public UIView<TData> Init(TData data)
        {
            Model = data;
            OnInit();
            return this;
        }

        protected abstract void OnInit();
    }
}
