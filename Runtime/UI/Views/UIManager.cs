using System;
using System.Collections.Generic;
using com.Plapamaru.Shared.Singletons;
using UnityEngine;

namespace com.Plapamaru.Shared.UI
{
    public class UIManager : MonoBehaviourSingleton<UIManager>
    {
        [SerializeField] private Canvas _canvas;

        private readonly Dictionary<Type, UIView> _views = new Dictionary<Type, UIView>();
        private readonly List<UIView> _popups = new List<UIView>();

        public Vector2 CanvasSize => ((RectTransform)Canvas.rootCanvas.transform).rect.size;
        public UIView TopPopup => _popups.Count > 0 ? _popups[^1] : null;

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
            if (_views.TryGetValue(type, out var existing))
            {
                if (existing != view)
                    Debug.LogError($"{type.Name} is already registered on '{existing.name}'. Ignoring '{view.name}'.", view);
                return;
            }

            _views.Add(type, view);
            view.Attach(this);
        }

        public void Unregister(UIView view)
        {
            if (_views.TryGetValue(view.GetType(), out var existing) && existing == view)
                _views.Remove(view.GetType());

            _popups.Remove(view);
            view.Detach();
        }

        public T Get<T>() where T : UIView
        {
            if (_views.TryGetValue(typeof(T), out var view))
                return (T)view;

            throw new KeyNotFoundException($"No {typeof(T).Name} is registered with {nameof(UIManager)}.");
        }

        public bool TryGet<T>(out T view) where T : UIView
        {
            if (_views.TryGetValue(typeof(T), out var found))
            {
                view = (T)found;
                return true;
            }

            view = null;
            return false;
        }

        public bool HandleBack()
        {
            var top = TopPopup;
            if (top == null)
                return false;

            top.OnBack();
            return true;
        }

        internal void AddPopup(UIView view)
        {
            _popups.Remove(view);
            _popups.Add(view);
        }

        internal void RemovePopup(UIView view)
        {
            _popups.Remove(view);
        }
    }
}
