using System.Collections.Generic;
using System.Diagnostics;
using com.Plapamaru.Shared.Singletons;
using com.Plapamaru.Shared.Utilities;
using UnityEngine;

namespace com.Plapamaru.Shared.Debugging
{
    public class GizmosController : MonoBehaviourSingleton<GizmosController>
    {
        private readonly Dictionary<string, Element> _elements = new Dictionary<string, Element>();

        public int Count => _elements.Count;

        [Conditional("UNITY_EDITOR")]
        public void DrawSphere(int id, Vector3 position, float radius, Color color) => DrawSphere(id.ToString(), position, radius, color);

        [Conditional("UNITY_EDITOR")]
        public void DrawSphere(string id, Vector3 position, float radius, Color color)
        {
            _elements[id] = new Element(Shape.Sphere, position, Vector3.zero, radius, color);
        }

        [Conditional("UNITY_EDITOR")]
        public void DrawWireSphere(string id, Vector3 position, float radius, Color color)
        {
            _elements[id] = new Element(Shape.WireSphere, position, Vector3.zero, radius, color);
        }

        [Conditional("UNITY_EDITOR")]
        public void DrawLine(int id, Vector3 from, Vector3 to, Color color) => DrawLine(id.ToString(), from, to, color);

        [Conditional("UNITY_EDITOR")]
        public void DrawLine(string id, Vector3 from, Vector3 to, Color color)
        {
            _elements[id] = new Element(Shape.Line, from, to, 0f, color);
        }

        [Conditional("UNITY_EDITOR")]
        public void DrawArrow(string id, Vector3 from, Vector3 to, float headSize, Color color)
        {
            _elements[id] = new Element(Shape.Arrow, from, to, headSize, color);
        }

        [Conditional("UNITY_EDITOR")]
        public void Remove(int id) => _elements.Remove(id.ToString());

        [Conditional("UNITY_EDITOR")]
        public void Remove(string id) => _elements.Remove(id);

        [Conditional("UNITY_EDITOR")]
        public void Clear() => _elements.Clear();

        private void OnDrawGizmos()
        {
            foreach (Element element in _elements.Values)
                element.Draw();
        }

        private enum Shape
        {
            Sphere,
            WireSphere,
            Line,
            Arrow
        }

        private readonly struct Element
        {
            private readonly Shape _shape;
            private readonly Vector3 _a;
            private readonly Vector3 _b;
            private readonly float _size;
            private readonly Color _color;

            public Element(Shape shape, Vector3 a, Vector3 b, float size, Color color)
            {
                _shape = shape;
                _a = a;
                _b = b;
                _size = size;
                _color = color;
            }

            public void Draw()
            {
                Gizmos.color = _color;
                switch (_shape)
                {
                    case Shape.Sphere:
                        Gizmos.DrawSphere(_a, _size);
                        break;
                    case Shape.WireSphere:
                        Gizmos.DrawWireSphere(_a, _size);
                        break;
                    case Shape.Line:
                        Gizmos.DrawLine(_a, _b);
                        break;
                    case Shape.Arrow:
                        GizmoUtils.DrawArrow(_a, _b, _size);
                        break;
                }
            }
        }
    }
}
