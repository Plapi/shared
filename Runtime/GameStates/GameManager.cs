using com.Plapamaru.Shared.Singletons;
using UnityEngine;

namespace com.Plapamaru.Shared.GameStates
{
    public abstract class GameManager<T> : MonoBehaviourSingleton<T> where T : GameManager<T>
    {
        [SerializeField] private bool _logStateChanges;

        public GameStateMachine Machine { get; } = new GameStateMachine();

        protected abstract GameStateBase CreateInitialState();

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this)
                return;

            Machine.StateChanged += OnStateChanged;
        }

        protected virtual void Start()
        {
            if (Instance == this)
                Machine.Change(CreateInitialState());
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Machine.StateChanged -= OnStateChanged;
            Machine.Dispose();
        }

        private void OnStateChanged()
        {
            if (_logStateChanges)
                Debug.Log($"[{typeof(T).Name}] {(Machine.States.Count > 0 ? string.Join(" > ", Machine.States) : "-")}", this);
        }
    }
}
