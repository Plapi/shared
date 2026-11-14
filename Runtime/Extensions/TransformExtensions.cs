using System;
using UnityEngine;

namespace com.Plapamaru.Shared.Extensions
{
    public static class TransformExtensions
    {
        public static void SetX(this Transform transform, float x)
        {
            Vector3 p = transform.position;
            p.x = x;
            transform.position = p;
        }

        public static void SetY(this Transform transform, float y)
        {
            Vector3 p = transform.position;
            p.y = y;
            transform.position = p;
        }

        public static void SetZ(this Transform transform, float z)
        {
            Vector3 p = transform.position;
            p.z = z;
            transform.position = p;
        }

        public static void SetXY(this Transform transform, float x, float y)
        {
            transform.position = new Vector3(x, y, transform.position.z);
        }

        public static void SetXZ(this Transform transform, float x, float z)
        {
            transform.position = new Vector3(x, transform.position.y, z);
        }

        public static void SetYZ(this Transform transform, float y, float z)
        {
            transform.position = new Vector3(transform.position.x, y, z);
        }

        public static void SetLocalX(this Transform transform, float x)
        {
            Vector3 p = transform.localPosition;
            p.x = x;
            transform.localPosition = p;
        }

        public static void SetLocalY(this Transform transform, float y)
        {
            Vector3 p = transform.localPosition;
            p.y = y;
            transform.localPosition = p;
        }

        public static void SetLocalZ(this Transform transform, float z)
        {
            Vector3 p = transform.localPosition;
            p.z = z;
            transform.localPosition = p;
        }

        public static void SetLocalXY(this Transform transform, float x, float y)
        {
            transform.localPosition = new Vector3(x, y, transform.localPosition.z);
        }

        public static void SetLocalXZ(this Transform transform, float x, float z)
        {
            transform.localPosition = new Vector3(x, transform.localPosition.y, z);
        }

        public static void SetLocalYZ(this Transform transform, float y, float z)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, y, z);
        }

        public static void SetScale(this Transform transform, float scale)
        {
            transform.localScale = new Vector3(scale, scale, scale);
        }

        public static void SetScaleX(this Transform transform, float x)
        {
            Vector3 s = transform.localScale;
            s.x = x;
            transform.localScale = s;
        }

        public static void SetScaleY(this Transform transform, float y)
        {
            Vector3 s = transform.localScale;
            s.y = y;
            transform.localScale = s;
        }

        public static void SetScaleZ(this Transform transform, float z)
        {
            Vector3 s = transform.localScale;
            s.z = z;
            transform.localScale = s;
        }

        public static void SetAngleX(this Transform transform, float x)
        {
            Vector3 e = transform.eulerAngles;
            e.x = x;
            transform.eulerAngles = e;
        }

        public static void SetAngleY(this Transform transform, float y)
        {
            Vector3 e = transform.eulerAngles;
            e.y = y;
            transform.eulerAngles = e;
        }

        public static void SetAngleZ(this Transform transform, float z)
        {
            Vector3 e = transform.eulerAngles;
            e.z = z;
            transform.eulerAngles = e;
        }

        public static void SetLocalAngleX(this Transform transform, float x)
        {
            Vector3 e = transform.localEulerAngles;
            e.x = x;
            transform.localEulerAngles = e;
        }

        public static void SetLocalAngleY(this Transform transform, float y)
        {
            Vector3 e = transform.localEulerAngles;
            e.y = y;
            transform.localEulerAngles = e;
        }

        public static void SetLocalAngleZ(this Transform transform, float z)
        {
            Vector3 e = transform.localEulerAngles;
            e.z = z;
            transform.localEulerAngles = e;
        }

        public static void SetLocalAngleXY(this Transform transform, float x, float y)
        {
            transform.localEulerAngles = new Vector3(x, y, transform.localEulerAngles.z);
        }

        public static void SetLocalAngleXZ(this Transform transform, float x, float z)
        {
            transform.localEulerAngles = new Vector3(x, transform.localEulerAngles.y, z);
        }

        public static void SetLocalAngleYZ(this Transform transform, float y, float z)
        {
            transform.localEulerAngles = new Vector3(transform.localEulerAngles.x, y, z);
        }

        public static void ForEachInHierarchy(this Transform root, Action<Transform> action)
        {
            action(root);
            foreach (Transform child in root)
                child.ForEachInHierarchy(action);
        }

        public static bool TryGetChild(this Transform transform, int index, out Transform child)
        {
            if (index >= 0 && index < transform.childCount)
            {
                child = transform.GetChild(index);
                return true;
            }
            child = null;
            return false;
        }

        public static void HideAllChildrenExcept(this Transform transform, int index)
        {
            for (int i = 0; i < transform.childCount; i++)
                transform.GetChild(i).gameObject.SetActive(i == index);

            if (index < 0 || index >= transform.childCount)
                Debug.LogError($"Child index {index} not found on '{transform.name}'.", transform);
        }
    }
}
