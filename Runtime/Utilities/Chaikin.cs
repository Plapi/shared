using System.Collections.Generic;
using UnityEngine;

namespace com.Plapamaru.Shared.Utilities
{
    public static class Chaikin
    {
        public static List<Vector3> SmoothPath(IReadOnlyList<Vector3> path, int passes)
        {
            List<Vector3> result = new List<Vector3>(path);
            for (int i = 0; i < passes; i++)
                result = SmoothPath(result);
            return result;
        }

        public static List<Vector3> SmoothPath(IReadOnlyList<Vector3> path)
        {
            var output = new List<Vector3>(path.Count * 2);

            if (path.Count > 0)
                output.Add(path[0]);

            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector3 p0 = path[i];
                Vector3 p1 = path[i + 1];
                output.Add(new Vector3(0.75f * p0.x + 0.25f * p1.x, 0f, 0.75f * p0.z + 0.25f * p1.z));
                output.Add(new Vector3(0.25f * p0.x + 0.75f * p1.x, 0f, 0.25f * p0.z + 0.75f * p1.z));
            }

            if (path.Count > 1)
                output.Add(path[^1]);

            return output;
        }
    }
}
