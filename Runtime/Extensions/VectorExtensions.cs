using UnityEngine;

namespace com.Plapamaru.Shared.Extensions
{
    public static class VectorExtensions
    {
        public static Vector2 ToXZ(this Vector3 v) => new Vector2(v.x, v.z);

        public static Vector3 XZToVector3(this Vector2 v, float y = 0f) => new Vector3(v.x, y, v.y);
    }
}
