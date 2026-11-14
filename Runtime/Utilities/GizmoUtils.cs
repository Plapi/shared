using UnityEngine;

namespace com.Plapamaru.Shared.Utilities
{
    public static class GizmoUtils
    {
        public static void DrawArrow(Vector3 start, Vector3 end, float headSize)
        {
            Gizmos.DrawLine(start, end);
            DrawArrowHead(start, end, headSize);
        }

        public static void DrawArrowHead(Vector3 start, Vector3 end, float size)
        {
            Vector3 direction = end - start;
            if (direction.sqrMagnitude < Mathf.Epsilon)
                return;

            Quaternion look = Quaternion.LookRotation(direction);
            Vector3 right = look * Quaternion.Euler(0, 35, 0) * Vector3.forward;
            Vector3 left = look * Quaternion.Euler(0, -35, 0) * Vector3.forward;
            Gizmos.DrawLine(end, end - right * size);
            Gizmos.DrawLine(end, end - left * size);
        }
    }
}
