using UnityEngine;

namespace com.Plapamaru.Shared.GameStates
{
    public abstract class GameManager : MonoBehaviour
    {
        [SerializeField] private bool _logStateChanges;

        protected GameStateMachine Machine { get; private set; }

        protected abstract void EnterInitialState();

        protected virtual void InitContext(GameStateBase.Context context)
        {
        }

        protected virtual void Awake()
        {
            Machine = new GameStateMachine(InitContext);
            Machine.StateChanged += OnStateChanged;
        }

        protected virtual void Start()
        {
            EnterInitialState();
        }

        protected virtual void OnDestroy()
        {
            Machine.StateChanged -= OnStateChanged;
            Machine.Dispose();
        }

        private void OnStateChanged()
        {
            if (_logStateChanges)
                Debug.Log($"[{GetType().Name}] {(Machine.States.Count > 0 ? string.Join(" > ", Machine.States) : "-")}", this);
        }
    }
}
