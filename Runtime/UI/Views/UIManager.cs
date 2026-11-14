using System;
using System.Collections.Generic;
using com.Plapamaru.Shared.Singletons;
using UnityEngine;

namespace com.Plapamaru.Shared.UI
{
    public class UIManager : MonoBehaviourSingleton<UIManager>
    {
        [SerializeField] private Canvas _canvas;

        private readonly Dictionary<Type, UIView> _viewsByType =new Dictionary<Type, UIView>();
        private readonly List<UIView> _viewStack = new List<UIView>();

        public Vector2 CanvasSize => ((RectTransform)Canvas.rootCanvas.transform).rect.size;
        public UIView TopView => _viewStack.Count > 0 ? _viewStack[^1] : null;

        private Canvas Canvas
        {
            get
            {
                if (_canvas == null)
                    _canvas = GetComponentInParent<Canvas>(true);

                return _canvas;
            }
        }

        protected override void Awake()
        {
            base.Awake();
            if (Instance != this)
                return;

            foreach (var view in GetComponentsInChildren<UIView>(true))
                Register(view);
        }

        public void Register(UIView view)
        {
            var type = view.GetType();
            if (_viewsByType.TryGetValue(type, out var existing))
            {
                if (existing != view)
                    Debug.LogError($"{type.Name} is already registered on '{existing.name}'. Ignoring '{view.name}'.", view);
                return;
            }

            _viewsByType.Add(type, view);
            view.Attach(this);
        }

        public void Unregister(UIView view)
        {
            if (_viewsByType.TryGetValue(view.GetType(), out var existing) && existing == view)
                _viewsByType.Remove(view.GetType());

            _viewStack.Remove(view);
            view.Detach();
        }

        public T Get<T>() where T : UIView
        {
            if (_viewsByType.TryGetValue(typeof(T), out var view))
                return (T)view;

            throw new KeyNotFoundException($"No {typeof(T).Name} is registered with {nameof(UIManager)}.");
        }

        public bool TryGet<T>(out T view) where T : UIView
        {
            if (_viewsByType.TryGetValue(typeof(T), out var found))
            {
                view = (T)found;
                return true;
            }

            view = null;
            return false;
        }

        public bool HandleBack()
        {
            var top = TopView;
            if (top == null)
                return false;

            top.OnBack();
            return true;
        }

        internal void PushToStack(UIView view)
        {
            _viewStack.Remove(view);
            _viewStack.Add(view);
        }

        internal void RemoveFromStack(UIView view)
        {
            _viewStack.Remove(view);
        }
    }
}
