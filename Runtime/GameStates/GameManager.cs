using UnityEngine;
#if SHARED_UGUI
using com.Plapamaru.Shared.UI;
#endif
#if SHARED_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace com.Plapamaru.Shared.GameStates
{
    public abstract class GameManager : MonoBehaviour
    {
        [SerializeField] private bool _logStateChanges;
        [SerializeField] private bool _backOnEscape = true;

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

        protected virtual void Update()
        {
            if (_backOnEscape && WasEscapePressed())
                HandleBack();
        }

        protected virtual void OnDestroy()
        {
            Machine.StateChanged -= OnStateChanged;
            Machine.Dispose();
        }

        protected virtual bool HandleBack()
        {
#if SHARED_UGUI
            if (UIManager.HasInstance() && UIManager.Instance.HandleBack())
                return true;
#endif
            return Machine.HandleBack();
        }

        private static bool WasEscapePressed()
        {
#if SHARED_INPUT_SYSTEM
            return Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame;
#elif ENABLE_LEGACY_INPUT_MANAGER
            return Input.GetKeyDown(KeyCode.Escape);
#else
            return false;
#endif
        }

        private void OnStateChanged()
        {
            if (_logStateChanges)
                Debug.Log($"[{GetType().Name}] {(Machine.States.Count > 0 ? string.Join(" > ", Machine.States) : "-")}", this);
        }
    }
}
