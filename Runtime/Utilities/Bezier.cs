using System.Collections.Generic;
using UnityEngine;

namespace com.Plapamaru.Shared.Utilities
{
    public static class Bezier
    {
        public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, float t)
        {
            t = Mathf.Clamp01(t);
            float u = 1f - t;
            return u * u * p0 + 2f * u * t * p1 + t * t * p2;
        }

        public static Vector3 GetPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
        {
            t = Mathf.Clamp01(t);
            float u = 1f - t;
            return u * u * u * p0 + 3f * u * u * t * p1 + 3f * u * t * t * p2 + t * t * t * p3;
        }

        public static Vector3 GetPoint(IReadOnlyList<Vector3> controlPoints, float t)
        {
            int n = controlPoints.Count - 1;
            if (n < 0)
                return Vector3.zero;

            t = Mathf.Clamp01(t);
            if (t <= 0f)
                return controlPoints[0];
            if (t >= 1f)
                return controlPoints[n];

            Vector3 point = Vector3.zero;
            float u = 1f - t;
            float binomial = 1f;
            for (int i = 0; i <= n; i++)
            {
                point += binomial * Mathf.Pow(t, i) * Mathf.Pow(u, n - i) * controlPoints[i];
                binomial = binomial * (n - i) / (i + 1);
            }
            return point;
        }

        public static List<Vector3> GetPoints(Vector3 p0, Vector3 p1, Vector3 p2, int segments)
        {
            segments = Mathf.Max(1, segments);
            var points = new List<Vector3>(segments + 1);
            for (int i = 0; i <= segments; i++)
                points.Add(GetPoint(p0, p1, p2, (float)i / segments));
            return points;
        }

        public static List<Vector3> GetPoints(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, int segments)
        {
            segments = Mathf.Max(1, segments);
            var points = new List<Vector3>(segments + 1);
            for (int i = 0; i <= segments; i++)
                points.Add(GetPoint(p0, p1, p2, p3, (float)i / segments));
            return points;
        }

        public static List<Vector3> GetPoints(IReadOnlyList<Vector3> controlPoints, int segments)
        {
            segments = Mathf.Max(1, segments);
            var points = new List<Vector3>(segments + 1);
            if (controlPoints.Count == 0)
                return points;

            for (int i = 0; i <= segments; i++)
                points.Add(GetPoint(controlPoints, (float)i / segments));
            return points;
        }
    }
}
