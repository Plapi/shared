using System;
using UnityEngine;

namespace com.Plapamaru.Shared.UI
{
    [Flags]
    public enum UISafeAreaEdges
    {
        None = 0,
        Left = 1,
        Right = 2,
        Top = 4,
        Bottom = 8,
        Horizontal = Left | Right,
        Vertical = Top | Bottom,
        All = Horizontal | Vertical
    }

    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    [AddComponentMenu("UI/Safe Area")]
    public class UISafeArea : MonoBehaviour
    {
        [SerializeField] private UISafeAreaEdges _edges = UISafeAreaEdges.All;
        [Tooltip("Inset left and right by the larger of the two, so content stays centred when only one side has a cutout.")]
        [SerializeField] private bool _symmetricHorizontal = true;

        public static Rect? SimulatedNormalized { get; set; }

        private RectTransform _rect;
        private DrivenRectTransformTracker _tracker;
        private Rect _appliedArea;
        private Rect? _appliedSimulated;
        private Vector2Int _appliedScreen;

        public UISafeAreaEdges Edges
        {
            get => _edges;
            set
            {
                _edges = value;
                Apply();
            }
        }

        private void OnEnable()
        {
            _rect = (RectTransform)transform;
            _tracker.Add(this, _rect, DrivenTransformProperties.Anchors | DrivenTransformProperties.AnchoredPosition | DrivenTransformProperties.SizeDelta);
            Apply();
        }

        private void OnDisable()
        {
            _tracker.Clear();
        }

        private void Update()
        {
            if (Screen.safeArea != _appliedArea || SimulatedNormalized != _appliedSimulated
                || Screen.width != _appliedScreen.x || Screen.height != _appliedScreen.y)
                Apply();
        }

        private void Apply()
        {
            if (_rect == null)
                return;

            _appliedArea = Screen.safeArea;
            _appliedSimulated = SimulatedNormalized;
            _appliedScreen = new Vector2Int(Screen.width, Screen.height);
            if (_appliedScreen.x <= 0 || _appliedScreen.y <= 0)
                return;

            var normalized = _appliedSimulated ?? new Rect(
                _appliedArea.x / _appliedScreen.x, _appliedArea.y / _appliedScreen.y,
                _appliedArea.width / _appliedScreen.x, _appliedArea.height / _appliedScreen.y);
            var min = normalized.min;
            var max = normalized.max;

            if (_symmetricHorizontal)
            {
                var inset = Mathf.Max(min.x, 1f - max.x);
                min.x = inset;
                max.x = 1f - inset;
            }

            if ((_edges & UISafeAreaEdges.Left) == 0)
                min.x = 0f;
            if ((_edges & UISafeAreaEdges.Right) == 0)
                max.x = 1f;
            if ((_edges & UISafeAreaEdges.Bottom) == 0)
                min.y = 0f;
            if ((_edges & UISafeAreaEdges.Top) == 0)
                max.y = 1f;

            _rect.anchorMin = min;
            _rect.anchorMax = max;
            _rect.offsetMin = Vector2.zero;
            _rect.offsetMax = Vector2.zero;
        }
    }
}
