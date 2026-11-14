using System.Collections.Generic;
using UnityEngine;

namespace com.Plapamaru.Shared.Utilities
{
    public static class GeometryUtils
    {
        public static Vector2 ToXZ(Vector3 v) => new Vector2(v.x, v.z);

        public static Vector3 FromXZ(Vector2 v, float y = 0f) => new Vector3(v.x, y, v.y);

        public static float DistancePointLine2D(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
        {
            return (ProjectPointLine2D(point, lineStart, lineEnd) - point).magnitude;
        }

        public static Vector2 ProjectPointLine2D(Vector2 point, Vector2 lineStart, Vector2 lineEnd)
        {
            Vector2 line = lineEnd - lineStart;
            float length = line.magnitude;
            if (length > 1E-06f)
                line /= length;
            float t = Mathf.Clamp(Vector2.Dot(line, point - lineStart), 0f, length);
            return lineStart + line * t;
        }

        public static float ClosestDistanceToPolygon(IReadOnlyList<Vector2> vertices, Vector2 point)
        {
            float minDistance = Mathf.Infinity;
            for (int i = 0, j = vertices.Count - 1; i < vertices.Count; j = i++)
                minDistance = Mathf.Min(minDistance, DistancePointLine2D(point, vertices[i], vertices[j]));
            return minDistance;
        }

        public static bool IsInsidePolygon(IReadOnlyList<Vector2> vertices, Vector2 point, float margin = 0.01f)
        {
            if (ClosestDistanceToPolygon(vertices, point) < margin)
                return true;

            bool inside = false;
            for (int i = 0, j = vertices.Count - 1; i < vertices.Count; j = i++)
            {
                Vector2 a = vertices[i];
                Vector2 b = vertices[j];
                if ((a.y <= point.y && point.y < b.y || b.y <= point.y && point.y < a.y) &&
                    point.x < (b.x - a.x) * (point.y - a.y) / (b.y - a.y) + a.x)
                    inside = !inside;
            }
            return inside;
        }

        public static bool IsInsidePolygonXZ(IReadOnlyList<Vector3> vertices, Vector3 point)
        {
            if (vertices.Count < 3)
                return false;

            int crossings = 0;
            Vector3 start = vertices[^1];
            start.x -= point.x;
            start.z -= point.z;

            bool currentSide = false;
            if (!ApproximatelyZero(start.z))
            {
                currentSide = start.z < 0f;
            }
            else
            {
                for (int i = vertices.Count - 2; i >= 0; --i)
                {
                    float z = vertices[i].z - point.z;
                    if (!ApproximatelyZero(z))
                    {
                        currentSide = z < 0f;
                        break;
                    }
                }
            }

            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 end = vertices[i];
                end.x -= point.x;
                end.z -= point.z;

                Vector3 edge = end - start;
                float edgeSqrMagnitude = edge.sqrMagnitude;
                if (ApproximatelyZero(edge.x * end.z - edge.z * end.x) &&
                    start.sqrMagnitude <= edgeSqrMagnitude && end.sqrMagnitude <= edgeSqrMagnitude)
                    return true;

                if (!ApproximatelyZero(end.z))
                {
                    bool nextSide = end.z < 0f;
                    if (nextSide != currentSide)
                    {
                        currentSide = nextSide;
                        if ((start.x * end.z - start.z * end.x) / -(start.z - end.z) > 0)
                            crossings++;
                    }
                }
                start = end;
            }
            return crossings % 2 > 0;
        }

        public static bool GetLineIntersectionXZ(Vector3 p0, Vector3 dir0, Vector3 p1, Vector3 dir1, out Vector3 intersection)
        {
            intersection = Vector3.zero;
            float det = dir0.x * dir1.z - dir0.z * dir1.x;
            if (Mathf.Abs(det) < Mathf.Epsilon)
                return false;

            Vector3 diff = p1 - p0;
            float t = (diff.x * dir1.z - diff.z * dir1.x) / det;
            intersection = p0 + t * dir0;
            return true;
        }

        public static Vector3 GetNearestPointsXZ(Vector3 p0, Vector3 p1, Vector3 target, Vector3 dir, out Vector3 leftPoint, out Vector3 rightPoint)
        {
            Vector2 point = GetNearestPoints(ToXZ(p0), ToXZ(p1), ToXZ(target), ToXZ(dir), out Vector2 left, out Vector2 right);
            leftPoint = FromXZ(left);
            rightPoint = FromXZ(right);
            return FromXZ(point);
        }

        public static Vector2 GetNearestPoints(Vector2 p0, Vector2 p1, Vector2 target, Vector2 dir, out Vector2 leftPoint, out Vector2 rightPoint)
        {
            float dirSqr = Vector2.Dot(dir, dir);
            Vector2 projP0 = p0 + Vector2.Dot(target - p0, dir) / dirSqr * dir;
            Vector2 projP1 = p1 + Vector2.Dot(target - p1, dir) / dirSqr * dir;

            Vector2 perpendicular = Vector2.Perpendicular(dir);
            Vector2 span = p1 - p0;
            float sign = perpendicular.x * span.y - perpendicular.y * span.x;

            if (sign > 0)
            {
                leftPoint = projP0;
                rightPoint = projP1;
            }
            else
            {
                leftPoint = projP1;
                rightPoint = projP0;
            }

            if (Vector2.Dot(target - leftPoint, rightPoint - leftPoint) >= 0 && Vector2.Dot(target - rightPoint, leftPoint - rightPoint) >= 0)
                return target;

            return Vector2.Dot(target - leftPoint, rightPoint - leftPoint) < 0 ? leftPoint : rightPoint;
        }

        public static float ComputeProgress(Vector3 targetPos, Vector3 left0, Vector3 right0, Vector3 left1, Vector3 right1)
        {
            Vector3 mid0 = (left0 + right0) * 0.5f;
            Vector3 mid1 = (left1 + right1) * 0.5f;
            Vector3 dir = mid1 - mid0;
            float length = dir.magnitude;
            if (length < Mathf.Epsilon)
                return 0f;

            return Mathf.Clamp01(Vector3.Dot(targetPos - mid0, dir / length) / length);
        }

        public static Vector3 KeepOnSideXZ(Vector3 point, Vector3 direction, Vector3 target)
        {
            Vector2 dir = ToXZ(direction).normalized;
            Vector2 perpendicular = new Vector2(-dir.y, dir.x);
            Vector2 pointToTarget = ToXZ(target - point);
            if (Vector2.Dot(pointToTarget, perpendicular) > 0)
                return target;

            float t = Vector2.Dot(pointToTarget, dir);
            return FromXZ(ToXZ(point) + t * dir);
        }

        public static Vector3 ClampDirection(Vector3 direction, Vector3 boundA, Vector3 boundB)
        {
            boundA.Normalize();
            boundB.Normalize();
            direction.Normalize();

            if (Vector3.Dot(direction, boundA) >= 0 && Vector3.Dot(direction, boundB) >= 0)
                return direction;

            return Vector3.Angle(direction, boundA) < Vector3.Angle(direction, boundB) ? boundA : boundB;
        }

        public static T GetClosest<T>(Vector3 origin, IReadOnlyList<T> targets) where T : Component
        {
            return GetExtreme(origin, targets, closest: true);
        }

        public static T GetFarthest<T>(Vector3 origin, IReadOnlyList<T> targets) where T : Component
        {
            return GetExtreme(origin, targets, closest: false);
        }

        private static T GetExtreme<T>(Vector3 origin, IReadOnlyList<T> targets, bool closest) where T : Component
        {
            if (targets == null)
                return null;

            T best = null;
            float bestSqrDistance = closest ? float.MaxValue : float.MinValue;
            for (int i = 0; i < targets.Count; i++)
            {
                T target = targets[i];
                if (target == null)
                    continue;

                float sqrDistance = (target.transform.position - origin).sqrMagnitude;
                if (closest ? sqrDistance < bestSqrDistance : sqrDistance > bestSqrDistance)
                {
                    bestSqrDistance = sqrDistance;
                    best = target;
                }
            }
            return best;
        }

        private static bool ApproximatelyZero(float value)
        {
            return Mathf.Abs(value) < Mathf.Epsilon;
        }
    }
}
