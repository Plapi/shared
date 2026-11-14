using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if SHARED_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace com.Plapamaru.Shared.UI
{
    public static class UIUtils
    {
        private static readonly List<RaycastResult> _results = new List<RaycastResult>(8);
        private static PointerEventData _pointerData;
        private static EventSystem _pointerDataOwner;

        public static bool IsPointerOverUI()
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null)
                return false;

#if SHARED_INPUT_SYSTEM
            if (Mouse.current != null && IsOverUI(eventSystem, Mouse.current.position.ReadValue()))
                return true;

            if (Touchscreen.current != null)
            {
                foreach (var touch in Touchscreen.current.touches)
                {
                    if (touch.isInProgress && IsOverUI(eventSystem, touch.position.ReadValue()))
                        return true;
                }
            }
#elif ENABLE_LEGACY_INPUT_MANAGER
            if (Input.mousePresent && IsOverUI(eventSystem, Input.mousePosition))
                return true;

            for (int i = 0; i < Input.touchCount; i++)
            {
                if (IsOverUI(eventSystem, Input.GetTouch(i).position))
                    return true;
            }
#endif
            return false;
        }

        public static bool IsOverUI(Vector2 screenPosition)
        {
            EventSystem eventSystem = EventSystem.current;
            return eventSystem != null && IsOverUI(eventSystem, screenPosition);
        }

        private static bool IsOverUI(EventSystem eventSystem, Vector2 screenPosition)
        {
            if (_pointerData == null || _pointerDataOwner != eventSystem)
            {
                _pointerDataOwner = eventSystem;
                _pointerData = new PointerEventData(eventSystem);
            }

            _pointerData.position = screenPosition;
            _results.Clear();
            eventSystem.RaycastAll(_pointerData, _results);

            foreach (RaycastResult result in _results)
            {
                if (result.module is GraphicRaycaster)
                    return true;
            }
            return false;
        }
    }
}
